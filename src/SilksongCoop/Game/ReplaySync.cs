using System.Collections.Generic;
using System.Linq;
using HutongGames.PlayMaker;
using SilksongCoop.Net;
using SilksongCoop.Rules;
using UnityEngine;

namespace SilksongCoop.Game;

/// <summary>
/// Room objects each game runs itself (explode walls, trap wires), 7.6: the state a hit or touch leads to
/// is replayed on the partner's copy, which plays the rest on its own. Their triggers need a hero nearby
/// or a camera check, so the partner's copy jumps past them. Each state plays once per room visit. A cut
/// platform's final cut (Vine Platform, 7.1) is replayed the same way. A crank platform that reaches its
/// end in one game is driven there in the other: each game runs its own climb, so a borderline hit differs.
/// </summary>
internal sealed class ReplaySync : Module
{
    private readonly HashSet<(int Id, string Fsm, string State)> _entered = [];
    private readonly HashSet<CrankPlat> _forced = [];
    private bool _applying;

    public ReplaySync(Coop coop)
        : base(coop) => coop.Rooms.Entered += OnEntered;

    public override void Register(Session session)
    {
        session.On(MessageType.StateReplayed, OnReplayed);
        session.On(MessageType.PlatformCut, OnPartnerCut);
        session.On(MessageType.CrankCompleted, OnPartnerCrank);
    }

    public override void Dispose() => Coop.Rooms.Entered -= OnEntered;

    public void OnSwitch(Fsm fsm, FsmState toState)
    {
        GameObject go = fsm.GameObject;
        if (_applying || !go || !SyncedObjects.IsReplayed(go.name, fsm.Name, toState.Name))
        {
            return;
        }
        (int Id, string Fsm, string State) key = (EnemySync.IdOf(go), fsm.Name, toState.Name);
        if (!_entered.Add(key) || !Coop.Rooms.PartnerHere || !Coop.Rooms.Holds(go.scene))
        {
            return;
        }
        int room = Coop.Rooms.CurrentHash;
        (int id, string fsmName, string state) = key;
        Session?.Send(MessageType.StateReplayed, w => w.Int(room).Int(id).String(fsmName).String(state));
    }

    public void OnLocalCut(SuspendedPlatformCut cut)
    {
        if (_applying || !Coop.Rooms.PartnerHere || !Coop.Rooms.Holds(cut.gameObject.scene))
        {
            return;
        }
        int room = Coop.Rooms.CurrentHash;
        int id = EnemySync.IdOf(cut.gameObject);
        Session?.Send(MessageType.PlatformCut, w => w.Int(room).Int(id));
    }

    /// <summary>A crank platform reached its end here, by a hit or the partner's completion.</summary>
    public void OnLocalCrank(CrankPlat plat)
    {
        if (_forced.Remove(plat) || !Coop.Rooms.PartnerHere || !Coop.Rooms.Holds(plat.gameObject.scene))
        {
            return;
        }
        int room = Coop.Rooms.CurrentHash;
        int id = EnemySync.IdOf(plat.gameObject);
        Session?.Send(MessageType.CrankCompleted, w => w.Int(room).Int(id));
    }

    private void OnEntered(string scene)
    {
        _entered.Clear();
        _forced.Clear();
    }

    private void OnPartnerCut(PacketReader reader)
    {
        int room = reader.Int();
        int id = reader.Int();
        SuspendedPlatformCut? cut = Object.FindObjectsByType<SuspendedPlatformCut>(FindObjectsSortMode.None)
            .FirstOrDefault(found => !found.activated && EnemySync.IdOf(found.gameObject) == id);
        if (room != Coop.Rooms.CurrentHash || cut is null)
        {
            return;
        }
        _applying = true;
        try
        {
            cut.Cut();
        }
        finally
        {
            _applying = false;
        }
    }

    private void OnPartnerCrank(PacketReader reader)
    {
        int room = reader.Int();
        int id = reader.Int();
        CrankPlat? plat = Object.FindObjectsByType<CrankPlat>(FindObjectsSortMode.None)
            .FirstOrDefault(found => !found.isComplete && EnemySync.IdOf(found.gameObject) == id);
        if (room != Coop.Rooms.CurrentHash || plat is null)
        {
            return;
        }
        // Its own climb finishes at the end point on the next frame: end audio, completion, re-arm below.
        _forced.Add(plat);
        plat.posT = 1f;
        plat.speed = Mathf.Max(plat.speed, plat.hitForce);
        plat.hitRoutine ??= plat.StartCoroutine(plat.HitRoutine());
    }

    private void OnReplayed(PacketReader reader)
    {
        int room = reader.Int();
        (int Id, string Fsm, string State) key = (reader.Int(), reader.String(), reader.String());
        if (room != Coop.Rooms.CurrentHash || !_entered.Add(key))
        {
            return;
        }
        PlayMakerFSM? target = Object.FindObjectsByType<PlayMakerFSM>(FindObjectsSortMode.None)
            .FirstOrDefault(fsm => fsm.FsmName == key.Fsm && EnemySync.IdOf(fsm.gameObject) == key.Id);
        if (target is null || !target.Fsm.HasState(key.State))
        {
            // Already gone here (the wall exploded on an earlier visit).
            Coop.Log.LogInfo($"partner replayed {key.Fsm}/{key.State} ({key.Id:x8}); no copy here");
            return;
        }
        _applying = true;
        try
        {
            target.SetState(key.State);
        }
        finally
        {
            _applying = false;
        }
    }
}
