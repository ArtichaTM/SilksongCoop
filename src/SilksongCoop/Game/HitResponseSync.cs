using System.Collections.Generic;
using SilksongCoop.Net;
using UnityEngine;

namespace SilksongCoop.Game;

/// <summary>
/// Room objects a hit drives (crank platforms, switches, anything with a <c>HitResponse</c>), 1.3 a: a hit
/// the game took is replayed on the partner's copy, so its events and effects run there too. Enemies,
/// breakables and FSMs that <see cref="EnemySync"/> already syncs are left to their own sync.
/// </summary>
internal sealed class HitResponseSync : Module
{
    private const float ReindexCooldown = 1f;

    private readonly Dictionary<int, HitResponse> _targets = [];
    private float _lastIndex = -ReindexCooldown;

    public HitResponseSync(Coop coop)
        : base(coop) => coop.Rooms.Entered += OnEntered;

    /// <summary>Replaying a partner's hit: no echo back, and no recoil for the local hero.</summary>
    public static bool Applying { get; private set; }

    public override void Register(Session session) => session.On(MessageType.ObjectHit, OnPartnerHit);

    public override void Dispose() => Coop.Rooms.Entered -= OnEntered;

    /// <summary>The game took this hit: it passed every filter and its effects ran.</summary>
    public void OnLocalHit(HitResponse target, HitInstance hit)
    {
        if (Applying || !Coop.Rooms.PartnerHere || !Synced(target))
        {
            return;
        }
        int id = EnemySync.IdOf(target.gameObject);
        Session?.Send(MessageType.ObjectHit, w => w
            .Int(id)
            .Int((int)hit.AttackType)
            .Int((int)hit.SpecialType)
            .Int((int)hit.HitEffectsType)
            .Float(hit.Direction)
            .Float(hit.MagnitudeMultiplier)
            .Bool(hit.IsHeroDamage)
            .Bool(hit.IsFirstHit));
    }

    private bool Synced(HitResponse target)
    {
        GameObject go = target.gameObject;
        bool ownSync = go.GetComponentInParent<HealthManager>(true) || target.passHitToBreakable;
        if (!Coop.Rooms.Holds(go.scene) || ownSync)
        {
            return false;
        }
        return !target.fsmTarget || Coop.Enemies.ModeOf(target.fsmTarget.Fsm) == FsmMode.None;
    }

    private void OnEntered(string scene)
    {
        _targets.Clear();
        _lastIndex = -ReindexCooldown;
    }

    private void OnPartnerHit(PacketReader reader)
    {
        int id = reader.Int();
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
        if (!Coop.Rooms.PartnerHere || Find(id) is not { } target || GameRefs.Hero is not { } hero)
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

    private HitResponse? Find(int id)
    {
        if (!_targets.TryGetValue(id, out HitResponse? target) || !target)
        {
            Reindex();
            _targets.TryGetValue(id, out target);
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
        foreach (HitResponse target in Object.FindObjectsByType<HitResponse>(FindObjectsSortMode.None))
        {
            if (Coop.Rooms.Holds(target.gameObject.scene))
            {
                _targets[EnemySync.IdOf(target.gameObject)] = target;
            }
        }
    }
}
