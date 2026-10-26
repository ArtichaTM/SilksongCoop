using System;
using System.Collections.Generic;
using System.Linq;
using SilksongCoop.Net;
using UnityEngine;

namespace SilksongCoop.Game;

/// <summary>
/// Room objects a hit drives (crank platforms, switches, anything with a <c>HitResponse</c>), 1.3 a: a hit
/// the game took is replayed on the partner's copy, so its events and effects run there too. Enemies,
/// breakables and FSMs that <see cref="EnemySync"/> already syncs are left to their own sync. Rosary caches
/// and health cocoons (2.22, 2.44) take hits the same way: each game flings its own loot, and each player
/// collects their own copy. A thread spinner shrinks in both games, but its silk goes to the hitter only.
/// </summary>
internal sealed class HitResponseSync : Module
{
    private const float ReindexCooldown = 1f;

    /// <summary>The hit responders replayed here; the index is the kind byte on the wire.</summary>
    private static readonly Type[] Kinds =
        [typeof(HitResponse), typeof(RosaryCache), typeof(HealthCocoon), typeof(ThreadSpinner)];

    private readonly Dictionary<(int Id, byte Kind), MonoBehaviour> _targets = [];
    private float _lastIndex = -ReindexCooldown;

    public HitResponseSync(Coop coop)
        : base(coop) => coop.Rooms.Entered += OnEntered;

    /// <summary>Replaying a partner's hit: no echo back, and no recoil for the local hero.</summary>
    public static bool Applying { get; private set; }

    public override void Register(Session session) => session.On(MessageType.ObjectHit, OnPartnerHit);

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

    private bool Synced(MonoBehaviour target)
    {
        GameObject go = target.gameObject;
        if (!Coop.Rooms.Holds(go.scene))
        {
            return false;
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
