using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using SilksongCoop.Net;
using UnityEngine;

namespace SilksongCoop.Game;

/// <summary>
/// Levers (<c>Lever_tk2d</c>: Great Bone Gate, Bone Gate, Chain Plat; <c>Lever</c>: Ant_17 trap bench and
/// 280 more), 1.4: neither an FSM nor a <c>HitResponse</c>, and their save flag is written only on leaving
/// the room. A pull in either game pulls the partner's copy, gates and all; a player arriving in a room gets
/// the levers already pulled there.
/// </summary>
internal sealed class LeverSync : Module
{
    private const float ReindexCooldown = 1f;

    private static readonly FieldInfo SetSaveState =
        AccessTools.Field(typeof(PersistentItem<bool>), "OnSetSaveState");

    private readonly Dictionary<int, Lever_tk2d> _levers = [];
    private readonly Dictionary<int, Lever> _plainLevers = [];
    private float _lastIndex = -ReindexCooldown;

    public LeverSync(Coop coop)
        : base(coop) => coop.Rooms.Entered += OnEntered;

    /// <summary>Pulling a lever because the partner did; the patch must not echo it back.</summary>
    public static bool Applying { get; private set; }

    public override void Register(Session session) => session.On(MessageType.LeverPulled, OnPulled);

    public override void Dispose() => Coop.Rooms.Entered -= OnEntered;

    public void OnLocalPull(Component lever)
    {
        if (!Applying && Coop.Rooms.PartnerHere && Coop.Rooms.Holds(lever.gameObject.scene))
        {
            Send(true, [EnemySync.IdOf(lever.gameObject)]);
        }
    }

    /// <summary>Called by the game that was in the room first when the partner arrives.</summary>
    public void SendSnapshot()
    {
        Reindex(force: true);
        List<int> pulled = _levers.Where(pair => pair.Value && pair.Value.activated)
            .Select(pair => pair.Key)
            .Concat(_plainLevers.Where(pair => pair.Value && pair.Value.activated).Select(pair => pair.Key))
            .ToList();
        if (pulled.Count > 0)
        {
            Send(false, pulled);
        }
    }

    private void Send(bool live, List<int> ids)
    {
        int room = Coop.Rooms.CurrentHash;
        Session?.Send(MessageType.LeverPulled, w =>
        {
            w.Int(room).Bool(live).Int(ids.Count);
            ids.ForEach(id => w.Int(id));
        });
    }

    private void OnEntered(string scene)
    {
        _levers.Clear();
        _plainLevers.Clear();
        _lastIndex = -ReindexCooldown;
    }

    private void OnPulled(PacketReader reader)
    {
        bool forThisRoom = reader.Int() == Coop.Rooms.CurrentHash;
        bool live = reader.Bool();
        int count = reader.Int();
        for (int i = 0; i < count; i++)
        {
            int id = reader.Int();
            if (!forThisRoom)
            {
                continue;
            }
            if (Find(id) is { activated: false } lever)
            {
                Pull(lever, live);
            }
            else if (FindPlain(id) is { activated: false } plain)
            {
                Pull(plain, live);
            }
        }
    }

    /// <summary>A live pull plays out with its gate opening; an earlier one loads as saved pulled.</summary>
    private static void Pull(Lever_tk2d lever, bool live)
    {
        Applying = true;
        try
        {
            if (!live && LoadedPulled(lever) is { } loaded)
            {
                loaded(true);
                return;
            }
            // The lever checks that the local hero stands where it can be hit; the partner stood there.
            TrackTriggerObjects range = lever.canHitTrigger;
            lever.canHitTrigger = null;
            try
            {
                GameObject? source = GameRefs.Hero?.gameObject;
                lever.Hit(new HitInstance { AttackType = AttackTypes.Nail, Source = source });
            }
            finally
            {
                lever.canHitTrigger = range;
            }
        }
        finally
        {
            Applying = false;
        }
    }

    private static void Pull(Lever lever, bool live)
    {
        Applying = true;
        try
        {
            if (!live)
            {
                lever.SetActivated(fromStart: true);
                return;
            }
            TrackTriggerObjects range = lever.canHitTrigger;
            lever.canHitTrigger = null;
            try
            {
                GameObject? source = GameRefs.Hero?.gameObject;
                HitInstance nail = new() { AttackType = AttackTypes.Nail, IsFirstHit = true };
                nail.Source = source;
                lever.Hit(nail);
            }
            finally
            {
                lever.canHitTrigger = range;
            }
        }
        finally
        {
            Applying = false;
        }
    }

    /// <summary>What a room load runs for a saved lever: pulled pose, gates open; null if unsaved.</summary>
    private static PersistentItem<bool>.SetValueEvent? LoadedPulled(Lever_tk2d lever)
    {
        bool saved = lever.TryGetComponent(out PersistentBoolItem item);
        return saved ? SetSaveState.GetValue(item) as PersistentItem<bool>.SetValueEvent : null;
    }

    private Lever_tk2d? Find(int id)
    {
        if (!_levers.TryGetValue(id, out Lever_tk2d? lever) || !lever)
        {
            Reindex(force: false);
            _levers.TryGetValue(id, out lever);
        }
        return lever ? lever : null;
    }

    private Lever? FindPlain(int id)
    {
        if (!_plainLevers.TryGetValue(id, out Lever? lever) || !lever)
        {
            Reindex(force: false);
            _plainLevers.TryGetValue(id, out lever);
        }
        return lever ? lever : null;
    }

    private void Reindex(bool force)
    {
        if ((!force && Time.unscaledTime - _lastIndex < ReindexCooldown) || Coop.Rooms.Current is null)
        {
            return;
        }
        _lastIndex = Time.unscaledTime;
        foreach (Lever_tk2d lever in Object.FindObjectsByType<Lever_tk2d>(FindObjectsSortMode.None))
        {
            if (Coop.Rooms.Holds(lever.gameObject.scene))
            {
                _levers[EnemySync.IdOf(lever.gameObject)] = lever;
            }
        }
        foreach (Lever lever in Object.FindObjectsByType<Lever>(FindObjectsSortMode.None))
        {
            if (Coop.Rooms.Holds(lever.gameObject.scene))
            {
                _plainLevers[EnemySync.IdOf(lever.gameObject)] = lever;
            }
        }
    }
}
