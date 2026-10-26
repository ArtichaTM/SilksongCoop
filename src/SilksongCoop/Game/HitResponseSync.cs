using System;
using System.Collections.Generic;
using System.Linq;
using HutongGames.PlayMaker;
using SilksongCoop.Net;
using UnityEngine;

namespace SilksongCoop.Game;

/// <summary>
/// Room objects a hit drives (crank platforms, switches, anything with a <c>HitResponse</c>), 1.3 a: a hit
/// the game took is replayed on the partner's copy, so its events and effects run there too. Enemies,
/// breakables and FSMs that <see cref="EnemySync"/> already syncs are left to their own sync. Rosary caches
/// and health cocoons (2.22, 2.44) take hits the same way: each game flings its own loot, and each player
/// collects their own copy. A thread spinner shrinks in both games, but its silk goes to the hitter only.
/// Geo rocks (3.12) take hits in their FSM, so the hit state is replayed instead, loot per player too.
/// Swinging bodies (hanging sacks, chains) get the same push, so a pogo target hangs where both see it.
/// An extracted lifeblood pustule (5.32) withers its vines in both games; its lifeblood stays the taker's.
/// A bounce pod's bounce or side hit plays its animation and recoil on the partner's copy, but not its short
/// inert time, which would stop the partner's own pogo.
/// </summary>
internal sealed class HitResponseSync : Module
{
    private const float ReindexCooldown = 1f;
    private const string GeoRockFsm = "Geo Rock";
    private const string HitsVar = "Hits";
    private const string DirectionVar = "Attack Direction";

    /// <summary>Every hit a geo rock takes enters one of these: they count hits down and fling geo.</summary>
    private static readonly string[] GeoRockHits = ["Hit", "Heavy Break"];

    /// <summary>The hit responders replayed here; the index is the kind byte on the wire.</summary>
    private static readonly Type[] Kinds =
    [
        typeof(HitResponse), typeof(RosaryCache), typeof(HealthCocoon), typeof(ThreadSpinner),
        typeof(HitRigidbody2D),
    ];

    private readonly Dictionary<(int Id, byte Kind), MonoBehaviour> _targets = [];
    private float _lastIndex = -ReindexCooldown;

    public HitResponseSync(Coop coop)
        : base(coop) => coop.Rooms.Entered += OnEntered;

    /// <summary>Replaying a partner's hit: no echo back, and no recoil for the local hero.</summary>
    public static bool Applying { get; private set; }

    public override void Register(Session session)
    {
        session.On(MessageType.ObjectHit, OnPartnerHit);
        session.On(MessageType.GeoRockHit, OnPartnerGeoRockHit);
        session.On(MessageType.PustuleExtracted, OnPartnerExtract);
        session.On(MessageType.BouncePodHit, OnPartnerPodHit);
    }

    public override void Dispose() => Coop.Rooms.Entered -= OnEntered;

    /// <summary>The game took this hit: it passed every filter and its effects ran.</summary>
    public void OnLocalHit(MonoBehaviour target, HitInstance hit)
    {
        if (Applying || !Coop.Rooms.PartnerHere || !Synced(target))
        {
            return;
        }
        int id = EnemySync.IdOf(target.gameObject);
        byte kind = KindOf(target);
        Session?.Send(MessageType.ObjectHit, w => w
            .Int(id)
            .Byte(kind)
            .Int((int)hit.AttackType)
            .Int((int)hit.SpecialType)
            .Int((int)hit.HitEffectsType)
            .Float(hit.Direction)
            .Float(hit.MagnitudeMultiplier)
            .Bool(hit.IsHeroDamage)
            .Bool(hit.IsFirstHit));
    }

    /// <summary>Called before any FSM switches state.</summary>
    public void OnSwitch(Fsm fsm, FsmState toState)
    {
        GameObject go = fsm.GameObject;
        bool hit = fsm.Name == GeoRockFsm && GeoRockHits.Contains(toState.Name);
        if (Applying || !hit || !go || !Coop.Rooms.PartnerHere || !Coop.Rooms.Holds(go.scene))
        {
            return;
        }
        int room = Coop.Rooms.CurrentHash;
        int id = EnemySync.IdOf(go);
        float direction = fsm.Variables.GetFsmFloat(DirectionVar).Value;
        Session?.Send(MessageType.GeoRockHit, w => w.Int(room).Int(id).String(toState.Name).Float(direction));
    }

    /// <summary>The local hero extracted this pustule: it broke for good.</summary>
    public void OnLocalExtract(LifebloodPustule pustule)
    {
        if (Applying || !Coop.Rooms.PartnerHere || !Coop.Rooms.Holds(pustule.gameObject.scene))
        {
            return;
        }
        int room = Coop.Rooms.CurrentHash;
        int id = EnemySync.IdOf(pustule.gameObject);
        Session?.Send(MessageType.PustuleExtracted, w => w.Int(room).Int(id));
    }

    /// <param name="bounce">A pogo off it, else a side hit.</param>
    /// <param name="push">Sign of the pod's recoil on X.</param>
    public void OnLocalPodHit(BouncePod pod, bool bounce, float push)
    {
        if (!Coop.Rooms.PartnerHere || !Coop.Rooms.Holds(pod.gameObject.scene))
        {
            return;
        }
        int room = Coop.Rooms.CurrentHash;
        int id = EnemySync.IdOf(pod.gameObject);
        Session?.Send(MessageType.BouncePodHit, w => w.Int(room).Int(id).Bool(bounce).Float(push));
    }

    private bool Synced(MonoBehaviour target)
    {
        GameObject go = target.gameObject;
        if (!Coop.Rooms.Holds(go.scene))
        {
            return false;
        }
        if (target is HitRigidbody2D)
        {
            return !go.GetComponentInParent<HealthManager>(true);
        }
        if (target is not HitResponse response)
        {
            return true;
        }
        bool ownSync = go.GetComponentInParent<HealthManager>(true) || response.passHitToBreakable;
        PlayMakerFSM? fsm = response.fsmTarget;
        return !ownSync && (!fsm || Coop.Enemies.ModeOf(fsm!.Fsm) == FsmMode.None);
    }

    private bool InRoom(MonoBehaviour target) => Coop.Rooms.Holds(target.gameObject.scene);

    private static byte KindOf(MonoBehaviour target) =>
        (byte)Array.FindIndex(Kinds, kind => kind.IsInstanceOfType(target));

    private void OnEntered(string scene)
    {
        _targets.Clear();
        _lastIndex = -ReindexCooldown;
    }

    private void OnPartnerHit(PacketReader reader)
    {
        int id = reader.Int();
        byte kind = reader.Byte();
        HitInstance hit = new()
        {
            AttackType = (AttackTypes)reader.Int(),
            SpecialType = (SpecialTypes)reader.Int(),
            HitEffectsType = (EnemyHitEffectsProfile.EffectsTypes)reader.Int(),
            Direction = reader.Float(),
            MagnitudeMultiplier = reader.Float(),
            IsHeroDamage = reader.Bool(),
            IsFirstHit = reader.Bool(),
        };
        IHitResponder? target = Find(id, kind) as IHitResponder;
        if (!Coop.Rooms.PartnerHere || target is null || GameRefs.Hero is not { } hero)
        {
            return;
        }
        Transform? partner = Coop.Heroes.PartnerTransform;
        hit.Source = partner ? partner!.gameObject : hero.gameObject;
        Applying = true;
        try
        {
            target.Hit(hit);
        }
        finally
        {
            Applying = false;
        }
    }

    private void OnPartnerGeoRockHit(PacketReader reader)
    {
        int room = reader.Int();
        int id = reader.Int();
        string state = reader.String();
        float direction = reader.Float();
        PlayMakerFSM? fsm = UnityEngine.Object.FindObjectsByType<GeoRock>(FindObjectsSortMode.None)
            .Where(rock => EnemySync.IdOf(rock.gameObject) == id)
            .Select(rock => rock.GetComponent<PlayMakerFSM>())
            .FirstOrDefault(found => found && found.FsmName == GeoRockFsm);
        // A rock already broken here (an earlier visit, or the hits crossed) takes no more.
        if (room != Coop.Rooms.CurrentHash || fsm is null || fsm.FsmVariables.GetFsmInt(HitsVar).Value <= 0)
        {
            return;
        }
        fsm.FsmVariables.GetFsmFloat(DirectionVar).Value = direction;
        Applying = true;
        try
        {
            fsm.SetState(state);
        }
        finally
        {
            Applying = false;
        }
    }

    /// <summary>Breaks the copy as an extract does, without the hero pin and the lifeblood.</summary>
    private void OnPartnerExtract(PacketReader reader)
    {
        int room = reader.Int();
        int id = reader.Int();
        LifebloodPustule? pustule = UnityEngine.Object
            .FindObjectsByType<LifebloodPustule>(FindObjectsSortMode.None)
            .FirstOrDefault(found => !found.isExtracted && EnemySync.IdOf(found.gameObject) == id);
        if (room != Coop.Rooms.CurrentHash || pustule is null)
        {
            return;
        }
        Applying = true;
        try
        {
            pustule.isExtracting = false;
            pustule.isExtracted = true;
            pustule.Break(pustule.EffectSpawnTransform, isExtracted: true);
        }
        finally
        {
            Applying = false;
        }
    }

    private void OnPartnerPodHit(PacketReader reader)
    {
        int room = reader.Int();
        int id = reader.Int();
        bool bounce = reader.Bool();
        float push = reader.Float();
        BouncePod? pod = UnityEngine.Object.FindObjectsByType<BouncePod>(FindObjectsSortMode.None)
            .FirstOrDefault(found => !found.isBroken && EnemySync.IdOf(found.gameObject) == id);
        if (room != Coop.Rooms.CurrentHash || pod is null)
        {
            return;
        }
        pod.PlayAnim(bounce ? pod.bounceAnim : pod.hitAnim);
        pod.SpawnHitEffects();
        if (pod.bounceReactBody)
        {
            Vector2 velocity = bounce ? pod.bounceReactVelocity : pod.hitReactVelocity;
            velocity.x *= push;
            pod.bounceReactBody.linearVelocity = velocity;
        }
    }

    private MonoBehaviour? Find(int id, byte kind)
    {
        if (!_targets.TryGetValue((id, kind), out MonoBehaviour? target) || !target)
        {
            Reindex();
            _targets.TryGetValue((id, kind), out target);
        }
        return target ? target : null;
    }

    private void Reindex()
    {
        if (Time.unscaledTime - _lastIndex < ReindexCooldown || Coop.Rooms.Current is null)
        {
            return;
        }
        _lastIndex = Time.unscaledTime;
        IEnumerable<MonoBehaviour> responders = Kinds
            .SelectMany(kind => UnityEngine.Object.FindObjectsByType(kind, FindObjectsSortMode.None))
            .OfType<MonoBehaviour>();
        foreach (MonoBehaviour target in responders.Where(InRoom))
        {
            _targets[(EnemySync.IdOf(target.gameObject), KindOf(target))] = target;
        }
    }
}
