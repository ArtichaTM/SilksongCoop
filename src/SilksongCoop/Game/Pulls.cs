using System.Linq;
using GlobalEnums;
using HutongGames.PlayMaker;
using SilksongCoop.Net;
using SilksongCoop.Rules;
using UnityEngine;

namespace SilksongCoop.Game;

/// <summary>
/// Keeps the players together. Walking too far is a closed door (D11, G5's native push-back); anything
/// that moves a player without a walk (fast travel, scripted scene changes, falls through a pit, same-room
/// teleports, an arena locking) brings the partner along (D8, D13, D47), downed or not (D36c).
/// </summary>
internal sealed class Pulls : Module
{
    private const float TeleportJump = 10f;
    private const float SnapWindow = 6f;
    private const float HintCooldown = 1.5f;

    private bool _applying;
    private bool _pullNextGate;
    private Vector3 _lastHero;
    private string? _lastScene;
    private string? _arenaLockedIn;
    private string? _snapScene;
    private float _snapUntil;
    private float _lastHint = -HintCooldown;
    private bool _joinPending;

    public Pulls(Coop coop)
        : base(coop) => coop.Rooms.Entered += OnEntered;

    public override void Register(Session session)
    {
        session.On(MessageType.Pull, OnPull);
        session.On(MessageType.Teleport, OnTeleport);
    }

    public override void Update()
    {
        if (!Coop.InCoop || GameRefs.Hero is not { } hero || Coop.Rooms.Current is not { } scene)
        {
            _lastScene = null;
            return;
        }
        if (_joinPending && GameRefs.HeroInControl)
        {
            _joinPending = false;
            JoinPartner();
        }
        SnapToPartner(hero, scene);
        Vector3 position = hero.transform.position;
        bool jumped = _lastScene == scene && Vector2.Distance(position, _lastHero) > TeleportJump;
        bool hazard = hero.cState.hazardRespawning || hero.doingHazardRespawn;
        if (jumped && !hazard && !Coop.Downed.IsDown && Coop.Rooms.PartnerHere && GameRefs.HeroReady)
        {
            Session?.Send(MessageType.Teleport, w => w.Float(position.x).Float(position.y));
        }
        _lastHero = position;
        _lastScene = scene;
    }

    public override void Dispose() => Coop.Rooms.Entered -= OnEntered;

    /// <summary>D31: once the friend can move (e.g. got up from the bench), it walks to the host.</summary>
    public void JoinPartnerWhenFree() => _joinPending = true;

    /// <summary>Dev tool: the partner comes to where this hero stands, from this room or any other.</summary>
    public void BringPartner()
    {
        if (Coop.Rooms.Current is { } scene)
        {
            // No gate: the partner enters by its usual door, then snaps to this hero.
            Session?.Send(MessageType.Pull, w => w.String(scene).String("").Bool(true));
        }
    }

    private void JoinPartner()
    {
        Partner partner = Coop.Partner;
        string? scene = partner.Scene ?? partner.Heading;
        if (scene is null || GameRefs.Manager is not { } gm)
        {
            return;
        }
        _snapScene = scene;
        _snapUntil = Time.unscaledTime + SnapWindow;
        if (scene == GameRefs.GameplayScene)
        {
            return;
        }
        Transition(gm, scene, PartnerGate());
    }

    /// <summary>H6: false keeps the hero in the room, pushed back like the native gate does.</summary>
    public bool AllowGate(TransitionPoint point, Collider2D? hero)
    {
        if (!Coop.InCoop || Coop.Rooms.Current is not { } scene || _applying)
        {
            return true;
        }
        TetherVerdict verdict = Tether.Check(scene, point.targetScene, Coop.Partner.TetherRoom);
        // Each game randomizes its own maze doors, so the partner's door list says nothing here.
        MazeController? maze = MazeController.NewestInstance;
        bool inMaze = maze && maze!.gameObject.scene.name == scene;
        if (verdict == TetherVerdict.Allowed || inMaze)
        {
            return true;
        }
        GatePosition side = point.GetGatePosition();
        if (hero is not null && side == GatePosition.bottom)
        {
            // D47: nothing stops a fall; the partner comes along instead.
            _pullNextGate = true;
            return true;
        }
        if (hero is not null)
        {
            PushBack(point, hero, side);
        }
        if (Time.unscaledTime - _lastHint > HintCooldown)
        {
            _lastHint = Time.unscaledTime;
            Coop.Hints.Show(Tether.Hint(verdict));
        }
        return false;
    }

    /// <summary>H7: every scene change starts here (G5).</summary>
    public void OnSceneTransition(GameManager.SceneLoadInfo info)
    {
        if (!Coop.InCoop)
        {
            return;
        }
        bool ownWay = Coop.Memories.EndsOnItsOwn(Coop.Rooms.Current) || Coop.BellBeast.TravelsTogether;
        Coop.Rooms.Leaving(info.SceneName);
        bool walked = info.GetType().DeclaringType == typeof(TransitionPoint);
        bool hazard = GameRefs.Hero?.doingHazardRespawn == true;
        bool exempt = _applying || Coop.Downed.RespawningTogether || hazard || Coop.Joining.Loading;
        if (exempt || ownWay || (walked && !_pullNextGate))
        {
            return;
        }
        _pullNextGate = false;
        string scene = info.SceneName;
        string gate = info.EntryGateName ?? "";
        Session?.Send(MessageType.Pull, w => w.String(scene).String(gate).Bool(gate.Length == 0));
    }

    /// <summary>D13 via F6: an arena's gates closing pulls an outside partner in.</summary>
    public void OnFsmSwitch(Fsm fsm, FsmState toState)
    {
        bool closing = fsm.Name == "BG Control" && fsm.LastTransition?.EventName == "BG CLOSE";
        bool exempt = EnemySync.Applying || PartnerWaits.Locking;
        if (!closing || !Coop.InCoop || exempt || Coop.Rooms.Current is not { } scene)
        {
            return;
        }
        if (_arenaLockedIn == scene || Coop.Partner.Scene == scene)
        {
            return;
        }
        _arenaLockedIn = scene;
        string gate = GameRefs.Manager?.entryGateName ?? "";
        Session?.Send(MessageType.Pull, w => w.String(scene).String(gate).Bool(true));
    }

    private void OnEntered(string scene)
    {
        _arenaLockedIn = null;
        _lastScene = null;
    }

    private void OnPull(PacketReader reader)
    {
        string scene = reader.String();
        string gate = reader.String();
        bool snap = reader.Bool();
        Coop.Downed.BeforePull();
        if (snap)
        {
            _snapScene = scene;
            _snapUntil = Time.unscaledTime + SnapWindow;
        }
        if (scene == Coop.Rooms.Current || GameRefs.Manager is not { } gm)
        {
            return;
        }
        if (gate.Length == 0)
        {
            gate = PartnerGate();
        }
        Transition(gm, scene, gate);
    }

    /// <summary>The door the partner came in by, if it is a door; a bench or respawn point is not.</summary>
    private string PartnerGate()
    {
        Partner partner = Coop.Partner;
        string fallback = partner.Gates.FirstOrDefault() ?? "";
        return partner.Gates.Contains(partner.EntryGate) ? partner.EntryGate : fallback;
    }

    private void Transition(GameManager gm, string scene, string gate)
    {
        GameManager.SceneLoadInfo load = new()
        {
            SceneName = scene,
            EntryGateName = gate,
            Visualization = GameManager.SceneLoadVisualizations.Default,
            WaitForSceneTransitionCameraFade = true,
        };
        _applying = true;
        try
        {
            gm.BeginSceneTransition(load);
        }
        finally
        {
            _applying = false;
        }
    }

    private void OnTeleport(PacketReader reader)
    {
        Vector2 to = new(reader.Float(), reader.Float());
        if (Coop.Rooms.PartnerHere && GameRefs.Hero is { } hero)
        {
            MoveHero(hero, to);
        }
    }

    /// <summary>After a pull without a usable gate: stand where the partner stands.</summary>
    private void SnapToPartner(HeroController hero, string scene)
    {
        bool waiting = _snapScene == scene && Time.unscaledTime < _snapUntil;
        if (waiting && Coop.Partner.Scene == scene && GameRefs.HeroInControl && Coop.Heroes.PartnerTransform)
        {
            _snapScene = null;
            MoveHero(hero, Coop.Partner.Position);
        }
    }

    private void MoveHero(HeroController hero, Vector2 to)
    {
        Coop.Downed.MoveDown(to);
        if (Coop.Downed.IsDown)
        {
            return;
        }
        hero.transform.position = new Vector3(to.x, to.y, hero.transform.position.z);
        hero.rb2d.linearVelocity = Vector2.zero;
        _lastHero = hero.transform.position;
    }

    /// <summary>The same shove TransitionPoint gives a hero who may not pass.</summary>
    private static void PushBack(TransitionPoint point, Collider2D hero, GatePosition side)
    {
        if (hero.TryGetComponent(out Rigidbody2D body))
        {
            // Only the speed into the gate: zeroing the fall too pins the hero sliding along its edge.
            Vector2 speed = body.linearVelocity;
            bool sideways = side is GatePosition.left or GatePosition.right;
            body.linearVelocity = sideways ? new Vector2(0f, speed.y) : new Vector2(speed.x, 0f);
        }
        Bounds gate = point.collider.bounds;
        Bounds self = hero.bounds;
        Vector2 shove = side switch
        {
            GatePosition.right => new Vector2(gate.min.x - self.max.x, 0f),
            GatePosition.left => new Vector2(gate.max.x - self.min.x, 0f),
            GatePosition.top => new Vector2(0f, gate.min.y - self.max.y),
            _ => Vector2.zero,
        };
        hero.transform.Translate(shove, Space.World);
    }
}
