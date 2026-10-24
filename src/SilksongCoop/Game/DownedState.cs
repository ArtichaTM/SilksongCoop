using System.Collections;
using System.Linq;
using GlobalEnums;
using SilksongCoop.Net;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SilksongCoop.Game;

/// <summary>
/// Death becomes "down" while the partner stands (D15-D19, D33, D40, D43, D44). The hero is frozen with
/// the same toggles the game's own death uses (not hand-rolled physics, R1-R3) and brought back with the
/// game's hazard-respawn reset. Both down: the host runs the real death, the friend the real respawn.
/// </summary>
internal sealed class DownedState : Module
{
    private const float ReviveRange = 2.5f;
    private const float CocoonRadius = 0.6f;
    private const float Skin = 0.02f;
    private const float FriendDeathWait = 4f;
    private const int SelfReviveHealth = 1;

    private GameObject? _ownCocoon;
    private bool _rolling;
    private GameObject? _partnerCocoon;
    private Vector2 _spot;
    private float _spawnHeight;
    private bool _nonLethal;
    private bool _frost;
    private bool _downThroughPull;
    private bool _watchPartner;
    private bool _breakingForPartner;
    private PlayMakerFSM? _bindFsm;
    private float _quietFor;

    public DownedState(Coop coop)
        : base(coop) => coop.Rooms.Entered += OnEntered;

    /// <summary>Lets the next <c>HeroController.Die</c> call run the game's real death.</summary>
    public static bool AllowRealDie { get; private set; }

    public bool IsDown { get; private set; }

    /// <summary>A joint respawn after both went down is under way; it pulls nobody.</summary>
    public bool RespawningTogether { get; private set; }

    public override void Register(Session session)
    {
        session.On(MessageType.Downed, OnPartnerDowned);
        session.On(MessageType.Revive, reader => OnRevived(reader.Int()));
        session.On(MessageType.BothDown, _ => OnBothDown());
        session.On(MessageType.CocoonBroken, _ => OnPartnerBrokeCocoon());
        session.On(MessageType.GotUp, _ => OnPartnerGotUp());
        session.On(MessageType.ForceRevive, _ => OnSummoned());
    }

    public override void Update()
    {
        Coop.Hints.Footer = "";
        if (IsDown && GameRefs.Hero is { } hero)
        {
            KeyCode key = Coop.Config.CameraKey.Value;
            if (Input.GetKeyDown(key))
            {
                _watchPartner = !_watchPartner;
            }
            string watch = _watchPartner ? "back to your cocoon" : $"watch {Coop.Partner.Name}";
            Coop.Hints.Footer = $"{key}: {watch}";
            AimCamera(hero, _watchPartner ? Coop.Heroes.PartnerTransform : null);
            // Built here, not in GoDown: a death inside a physics callback can't strip the copy's logic.
            _ownCocoon ??= CocoonVisual.Spawn(_spot, Coop.Config.PlayerName.Value);
            Roll(hero);
            GetUpWhenQuiet(hero);
        }
        Partner partner = Coop.Partner;
        bool showPartnerCocoon = partner.Downed && partner.Scene is { } room && room == Coop.Rooms.Current;
        if (showPartnerCocoon)
        {
            partner.Cocoon = partner.Position;
            _partnerCocoon ??= CocoonVisual.Spawn(partner.Cocoon, partner.Name);
            _partnerCocoon.transform.position = new Vector3(partner.Cocoon.x, partner.Cocoon.y, 0.01f);
        }
        else
        {
            DestroyPartnerCocoon();
        }
    }

    public override void OnLeft()
    {
        DestroyPartnerCocoon();
        if (IsDown && Coop.IsHost && GameRefs.Hero is { } hero)
        {
            // Nobody left to revive us: the down becomes the real death.
            DieForReal(hero);
        }
    }

    public override void Dispose()
    {
        Coop.Hints.Footer = "";
        DestroyPartnerCocoon();
        DestroyOwnCocoon();
        if (GameRefs.Hero is { } hero)
        {
            AimCamera(hero, null);
        }
        Coop.Rooms.Entered -= OnEntered;
    }

    /// <summary>H1: replaces the death routine; false lets the real <c>Die</c> run.</summary>
    public bool TryGoDown(HeroController hero, bool nonLethal, bool frost, out IEnumerator? routine)
    {
        routine = null;
        if (!Coop.InCoop || AllowRealDie)
        {
            return false;
        }
        if (IsDown || _downThroughPull)
        {
            Log($"death ignored: already down={IsDown}, down through a pull={_downThroughPull}");
            routine = Nothing();
            return true;
        }
        if (Coop.Partner.Downed && Coop.IsHost)
        {
            // D36d: both down, the host's real death at its own spot.
            Log("died while the partner was down: real death for both");
            Session?.Send(MessageType.BothDown);
            Coop.Partner.Downed = false;
            RespawningTogether = true;
            return false;
        }
        _nonLethal = nonLethal;
        _frost = frost;
        GoDown(hero, hero.transform.position);
        routine = Nothing();
        return true;
    }

    /// <summary>H5: Bind's heal on a partner's cocoon revives (D17, D18); false skips the heal.</summary>
    public bool OnAddHealth(HeroController hero, int amount)
    {
        Partner partner = Coop.Partner;
        bool partnerHere = Coop.InCoop && partner.Downed && partner.Scene == Coop.Rooms.Current;
        if (!partnerHere || BindFsm(hero)?.ActiveStateName != "Heal")
        {
            return true;
        }
        if (Vector2.Distance(hero.transform.position, partner.Cocoon) > ReviveRange)
        {
            return true;
        }
        Session?.Send(MessageType.Revive, w => w.Int(amount));
        Log($"revived the partner with {amount} health");
        partner.Downed = false;
        DestroyPartnerCocoon();
        Coop.Hints.Show($"Revived {partner.Name}");
        return Coop.Config.ReviverHealed.Value;
    }

    /// <summary>The shared death cocoon broke here: the partner's game runs the same break.</summary>
    public void OnCocoonBroken()
    {
        if (!_breakingForPartner && Coop.InCoop)
        {
            Session?.Send(MessageType.CocoonBroken);
        }
    }

    /// <summary>The partner broke the shared cocoon: spool, silk and HUD as if we had.</summary>
    private void OnPartnerBrokeCocoon()
    {
        if (GameRefs.Hero is not { } hero)
        {
            return;
        }
        string scene = hero.playerData.HeroCorpseScene;
        hero.playerData.HeroCorpseMoneyPool = 0;
        _breakingForPartner = true;
        try
        {
            hero.CocoonBroken(false, false);
        }
        finally
        {
            _breakingForPartner = false;
        }
        RemoveCorpse(scene);
    }

    /// <summary>Our copy of the broken cocoon would give its silk a second time.</summary>
    private static void RemoveCorpse(string scene)
    {
        Scene loaded = SceneManager.GetSceneByName(scene);
        string name = GameRefs.Manager?.sm?.heroCorpsePrefab?.name + "(Clone)";
        if (!loaded.IsValid() || !loaded.isLoaded)
        {
            return;
        }
        foreach (GameObject root in loaded.GetRootGameObjects().Where(root => root.name == name))
        {
            Object.Destroy(root);
        }
    }

    /// <summary>D36c: a same-room teleport carries a downed player's cocoon along.</summary>
    public void MoveDown(Vector2 to)
    {
        if (!IsDown || GameRefs.Hero is not { } hero)
        {
            return;
        }
        _spot = to;
        _spawnHeight = to.y;
        hero.transform.position = new Vector3(to.x, to.y, hero.transform.position.z);
        if (_ownCocoon)
        {
            _ownCocoon!.transform.position = new Vector3(to.x, to.y, 0.01f);
        }
    }

    /// <summary>D36c: a pull moves a downed player along; the hero is thawed for the trip.</summary>
    public void BeforePull()
    {
        if (!IsDown || GameRefs.Hero is not { } hero)
        {
            return;
        }
        Log("pulled while down; going down again in the next room");
        Thaw(hero);
        IsDown = false;
        _downThroughPull = true;
        DestroyOwnCocoon();
    }

    private static IEnumerator Nothing()
    {
        yield break;
    }

    private void OnEntered(string scene)
    {
        RespawningTogether = false;
        if (_downThroughPull && GameRefs.Hero is { } hero)
        {
            _downThroughPull = false;
            GoDown(hero, hero.transform.position);
        }
    }

    private void GoDown(HeroController hero, Vector2 deathPosition)
    {
        Log($"down in {Coop.Rooms.Current} at {deathPosition}");
        IsDown = true;
        _quietFor = 0f;
        _watchPartner = false;
        _spot = CocoonSpot(deathPosition);
        _spawnHeight = _spot.y;
        Freeze(hero);
        hero.transform.position = new Vector3(_spot.x, _spot.y, hero.transform.position.z);
        _rolling = Coop.Config.CocoonMoves.Value;
        Vector2 spot = _spot;
        string scene = Coop.Rooms.Current ?? "";
        Session?.Send(MessageType.Downed, w => w.String(scene).Float(spot.x).Float(spot.y));
        Coop.Hints.Show($"Down: {Coop.Partner.Name} can revive you with Bind on your cocoon", 5f);
    }

    /// <summary>D44: where the game would put its own cocoon for a death here.</summary>
    private static Vector2 CocoonSpot(Vector2 deathPosition)
    {
        HeroCorpseMarker? closest = HeroCorpseMarker.GetClosest(deathPosition);
        return closest ? closest!.Position : deathPosition;
    }

    private void OnPartnerDowned(PacketReader reader)
    {
        _ = reader.String();
        Partner partner = Coop.Partner;
        partner.Downed = true;
        partner.Cocoon = new Vector2(reader.Float(), reader.Float());
        partner.Position = partner.Cocoon;
        Log($"partner down at {partner.Cocoon}");
        Coop.Hints.Show($"{partner.Name} is down: Bind on the cocoon to revive", 5f);
        if (IsDown && Coop.IsHost && GameRefs.Hero is { } hero)
        {
            Session?.Send(MessageType.BothDown);
            partner.Downed = false;
            DieForReal(hero);
        }
    }

    private void OnRevived(int amount)
    {
        if (IsDown && GameRefs.Hero is { } hero)
        {
            Log($"revived by the partner with {amount} health");
            GetUp(hero, amount);
            Coop.Hints.Show("Revived");
        }
    }

    /// <summary>With no enemy left there is no silk for a reviving Bind; the downed get up alone.</summary>
    private void GetUpWhenQuiet(HeroController hero)
    {
        float delay = Coop.Config.SelfReviveDelay.Value;
        if (delay <= 0f || !Coop.InCoop || Coop.Enemies.AnyEnemyActive)
        {
            _quietFor = 0f;
            return;
        }
        _quietFor += Time.deltaTime;
        if (_quietFor >= delay)
        {
            Log($"no enemies for {delay:0} s: got up alone");
            Session?.Send(MessageType.GotUp);
            GetUp(hero, ForcedHealth(hero));
            Coop.Hints.Show("No enemies around: you got up");
        }
    }

    /// <summary>9.1-9.4: a fight starts while the partner is down; it gets up and joins this hero.</summary>
    public void SummonPartner()
    {
        Log("partner is down as a fight starts: summoning it");
        Session?.Send(MessageType.ForceRevive);
    }

    private void OnSummoned()
    {
        if (!IsDown || GameRefs.Hero is not { } hero)
        {
            return;
        }
        Log($"summoned to {Coop.Partner.Name}'s fight: getting up");
        Session?.Send(MessageType.GotUp);
        GetUp(hero, ForcedHealth(hero));
        Coop.Pulls.JoinPartnerWhenFree();
        Coop.Hints.Show($"{Coop.Partner.Name} started a fight: you got up");
    }

    /// <summary>A revive without a Bind: full health unless the config says 1.</summary>
    private int ForcedHealth(HeroController hero) =>
        Coop.Config.ForceReviveFullHealth.Value ? hero.playerData.CurrentMaxHealth : SelfReviveHealth;

    private void OnPartnerGotUp()
    {
        Log("partner got up alone");
        Coop.Partner.Downed = false;
        DestroyPartnerCocoon();
    }

    private void GetUp(HeroController hero, int amount)
    {
        IsDown = false;
        Vector2 at = _spot;
        DestroyOwnCocoon();
        AimCamera(hero, null);
        hero.transform.position = new Vector3(at.x, at.y, hero.transform.position.z);
        Thaw(hero);
        hero.playerData.health = 0;
        hero.AddHealth(amount);
        hero.StartInvulnerable(1.5f);
    }

    /// <summary>Friend side of both down: the game's own respawn at the host's last respawn point.</summary>
    private void OnBothDown()
    {
        Log("both down: respawning");
        DestroyPartnerCocoon();
        Coop.Partner.Downed = false;
        if (Coop.IsHost || GameRefs.Manager is not { } gm || GameRefs.Hero is not { } hero)
        {
            return;
        }
        IsDown = false;
        _downThroughPull = false;
        DestroyOwnCocoon();
        AimCamera(hero, null);
        RespawningTogether = true;
        if (gm.IsMemoryScene())
        {
            // What Die does for a non-lethal death (F5, D43): leave the memory the way we came in.
            PlayerData data = hero.playerData;
            data.tempRespawnMarker = data.nonLethalRespawnMarker;
            data.tempRespawnScene = data.nonLethalRespawnScene;
            data.tempRespawnType = data.nonLethalRespawnType;
        }
        else
        {
            // Die's penalty for a real death; the cocoon and rosaries come with the host's save.
            hero.playerData.IsSilkSpoolBroken = true;
        }
        if (!hero.cState.dead)
        {
            Freeze(hero);
        }
        gm.StartCoroutine(gm.PlayerDead(FriendDeathWait));
    }

    private void DieForReal(HeroController hero)
    {
        Log("down became a real death");
        IsDown = false;
        RespawningTogether = Coop.InCoop;
        DestroyOwnCocoon();
        AimCamera(hero, null);
        hero.cState.dead = false;
        AllowRealDie = true;
        try
        {
            hero.StartCoroutine(hero.Die(_nonLethal, _frost));
        }
        finally
        {
            AllowRealDie = false;
        }
    }

    private void Log(string text) => Coop.Log.LogInfo($"downed: {text}");

    /// <summary>The same switches <c>HeroController.Die</c> flips, minus the death consequences.</summary>
    private static void Freeze(HeroController hero)
    {
        EventRegister.SendEvent(EventRegisterEvents.FsmCancel);
        hero.RelinquishControl();
        hero.rb2d.linearVelocity = Vector2.zero;
        hero.AffectedByGravity(false);
        HeroBox.Inactive = true;
        hero.cState.falling = false;
        hero.rb2d.bodyType = RigidbodyType2D.Kinematic;
        hero.SetState(ActorStates.no_input);
        hero.cState.dead = true;
        hero.ResetMotion();
        hero.ResetHardLandingTimer();
        hero.GetComponent<MeshRenderer>().enabled = false;
        hero.gameObject.layer = (int)PhysLayers.IGNORE_RAYCAST;
        hero.heroBox.HeroBoxOff();
    }

    /// <summary>G3: the game's own way back (hazard respawn reset, then scene entry finish).</summary>
    private static void Thaw(HeroController hero)
    {
        hero.cState.dead = false;
        HeroBox.Inactive = false;
        hero.rb2d.bodyType = RigidbodyType2D.Dynamic;
        hero.HazardRespawnReset();
        hero.cState.hazardRespawning = false;
        hero.FinishedEnteringScene(false, false);
        hero.RegainControl();
    }

    /// <summary>D19 option, deprecated (3.6 b): a slow float, at most CocoonRiseLimit up.</summary>
    private void Roll(HeroController hero)
    {
        if (!_rolling || !_ownCocoon)
        {
            return;
        }
        Vector2 step = hero.inputHandler.inputActions.MoveVector.Vector * Coop.Config.CocoonSpeed.Value
            * Time.deltaTime;
        _spot = Sweep(_spot, new Vector2(step.x, 0f));
        float ceiling = _spawnHeight + Coop.Config.CocoonRise.Value;
        _spot = Sweep(_spot, new Vector2(0f, Mathf.Min(step.y, ceiling - _spot.y)));
        hero.transform.position = new Vector3(_spot.x, _spot.y, hero.transform.position.z);
        _ownCocoon!.transform.position = new Vector3(_spot.x, _spot.y, 0.01f);
    }

    /// <summary>Moves a cocoon-sized circle up to terrain; a start inside terrain stays put.</summary>
    private static Vector2 Sweep(Vector2 from, Vector2 delta)
    {
        float distance = delta.magnitude;
        if (distance <= 0f)
        {
            return from;
        }
        const int Terrain = 1 << (int)PhysLayers.TERRAIN;
        RaycastHit2D hit = Physics2D.CircleCast(from, CocoonRadius, delta / distance, distance, Terrain);
        if (!hit)
        {
            return from + delta;
        }
        return from + (delta / distance * Mathf.Max(0f, hit.distance - Skin));
    }

    /// <summary>D19: the camera follows the cocoon (the hero) or the partner.</summary>
    private static void AimCamera(HeroController hero, Transform? partner)
    {
        if (GameCameras.instance is not { } cameras || !cameras || cameras.cameraTarget is not { } target)
        {
            return;
        }
        // Leaving a lock zone while "dead" frees the camera (CameraTarget.ExitLockZone); keep it following.
        if (target.mode == CameraTarget.TargetMode.FREE && !target.IsFreeModeManual)
        {
            target.mode = CameraTarget.TargetMode.FOLLOW_HERO;
        }
        Transform wanted = partner ? partner! : hero.transform;
        if (target.heroTransform != wanted)
        {
            target.heroTransform = wanted;
        }
    }

    private PlayMakerFSM? BindFsm(HeroController hero)
    {
        if (!_bindFsm || _bindFsm!.gameObject != hero.gameObject)
        {
            _bindFsm = hero.GetComponents<PlayMakerFSM>().FirstOrDefault(fsm => fsm.FsmName == "Bind");
        }
        return _bindFsm;
    }

    private void DestroyOwnCocoon()
    {
        if (_ownCocoon)
        {
            Object.Destroy(_ownCocoon);
        }
        _ownCocoon = null;
        _rolling = false;
    }

    private void DestroyPartnerCocoon()
    {
        if (_partnerCocoon)
        {
            Object.Destroy(_partnerCocoon);
        }
        _partnerCocoon = null;
    }
}
