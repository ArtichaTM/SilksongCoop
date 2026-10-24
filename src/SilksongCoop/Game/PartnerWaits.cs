using System;
using System.Collections.Generic;
using System.Linq;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using SilksongCoop.Net;
using SilksongCoop.Rules;
using UnityEngine;

namespace SilksongCoop.Game;

/// <summary>
/// The global wait rule (4.1 b): a fight or solo cutscene one player starts waits until both players stand
/// where it was started. It waits on a hero walking into a <c>BattleScene</c>, an FSM event whose next
/// states close arena gates, and the takeovers in <see cref="WaitPoints"/>. What cannot wait (a hit, a
/// timer, a battle an FSM starts mid-cutscene) starts at once and pulls the partner in (4.1 c, Pulls).
/// </summary>
internal sealed class PartnerWaits : Module
{
    private const float NearDistance = 10f;
    private const float HintInterval = 2.5f;
    private const int FinishedChainDepth = 8;
    private const string GatesClose = "BG CLOSE";

    private readonly Dictionary<(Fsm Fsm, string State, string Event), bool> _startsLock = [];
    private Held? _held;
    private float _nextHint;
    private bool _replaying;

    public PartnerWaits(Coop coop)
        : base(coop) => coop.Rooms.Entered += OnEntered;

    public override void Register(Session session) => session.On(MessageType.PartnerWait, OnPartnerWait);

    public override void Update()
    {
        if (_held is not { } held)
        {
            return;
        }
        if (!Coop.InCoop || Coop.Rooms.Current != held.Scene || !held.StillPending())
        {
            _held = null;
            return;
        }
        if (BothAt(held.Spot))
        {
            _held = null;
            Coop.Log.LogInfo($"wait over, starting {held.Name}");
            Replay(held);
            return;
        }
        if (Time.unscaledTime >= _nextHint)
        {
            _nextHint = Time.unscaledTime + HintInterval;
            ShowWaitHint(held.PartnerStarted);
            Session?.Send(MessageType.PartnerWait, w => w.Bool(held.PartnerStarted));
        }
    }

    public override void OnLeft() => _held = null;

    public override void Dispose() => Coop.Rooms.Entered -= OnEntered;

    /// <summary>Before an unmirrored FSM takes an event: true holds it back until both are here.</summary>
    public bool HoldsEvent(Fsm fsm, FsmEvent? fsmEvent)
    {
        bool plain = fsmEvent is null || fsmEvent == FsmEvent.Finished || !fsm.GameObject;
        if (_replaying || plain || fsm.ActiveState is not { } state || !StartsAlone(fsm, state, fsmEvent!))
        {
            return false;
        }
        string stateName = state.Name;
        string name = $"{fsm.GameObject!.name} [{fsm.Name}] {stateName} -{fsmEvent!.Name}->";
        Held held = new(name, () => fsm.ActiveStateName == stateName, () => fsm.Event(fsmEvent));
        Hold(held, EnemySync.Relaying);
        return true;
    }

    /// <summary>The local hero walked into a battle: true holds the start back.</summary>
    public bool HoldsBattle(BattleScene battle)
    {
        bool fresh = !battle.started && !battle.completed && battle.currentWave == 0;
        if (_replaying || !fresh || !MustWait())
        {
            return false;
        }
        Func<bool> pending = () => battle && !battle.started && !battle.completed;
        Hold(new Held(battle.name, pending, battle.StartBattle), false);
        return true;
    }

    /// <summary>Taking this event starts a fight or takeover that the partner must not miss.</summary>
    private bool StartsAlone(Fsm fsm, FsmState state, FsmEvent fsmEvent)
    {
        if (!MustWait())
        {
            return false;
        }
        (Fsm, string, string) key = (fsm, state.Name, fsmEvent.Name);
        if (!_startsLock.TryGetValue(key, out bool starts))
        {
            starts = WaitPoints.Waits(fsm.GameObject.name, fsm.Name, state.Name, fsmEvent.Name)
                || LeadsToLock(fsm, state, fsmEvent);
            _startsLock[key] = starts;
        }
        return starts;
    }

    /// <summary>Coop with nobody down (a downed player cannot come) and the partner not near.</summary>
    private bool MustWait()
    {
        if (!Coop.InCoop || Coop.Rooms.Current is null || Coop.Downed.IsDown || Coop.Partner.Downed)
        {
            return false;
        }
        return !(GameRefs.Hero is { } hero && PartnerNear(hero.transform.position));
    }

    private bool BothAt(Vector2 spot) =>
        PartnerNear(spot) && GameRefs.Hero is { } hero && Near(hero.transform.position, spot);

    private bool PartnerNear(Vector2 spot) => Coop.Rooms.PartnerHere && Near(Coop.Partner.Position, spot);

    private static bool Near(Vector2 a, Vector2 b) => Vector2.Distance(a, b) <= NearDistance;

    /// <summary>One wait at a time; the same start fired again (a trigger each frame) keeps it.</summary>
    private void Hold(Held held, bool partnerStarted)
    {
        if (_held is { } current && current.Name == held.Name)
        {
            return;
        }
        Vector2? spot = partnerStarted ? Coop.Partner.Position : GameRefs.Hero?.transform.position;
        if (spot is null)
        {
            return;
        }
        held.Scene = Coop.Rooms.Current;
        held.Spot = spot.Value;
        held.PartnerStarted = partnerStarted;
        _held = held;
        _nextHint = 0f;
        Coop.Log.LogInfo($"waiting for the partner: {held.Name} at {held.Spot}");
    }

    private void Replay(Held held)
    {
        _replaying = true;
        try
        {
            held.Start();
        }
        finally
        {
            _replaying = false;
        }
    }

    private void ShowWaitHint(bool partnerStarted)
    {
        string name = Coop.Partner.Name;
        string text = partnerStarted ? $"{name} is waiting for you" : $"Waiting for {name}";
        Coop.Hints.Show(text, HintInterval + 0.5f);
    }

    /// <summary>The partner holds a start back: the sender's flag says whether it was ours.</summary>
    private void OnPartnerWait(PacketReader reader) => ShowWaitHint(!reader.Bool());

    private void OnEntered(string scene)
    {
        _held = null;
        _startsLock.Clear();
    }

    /// <summary>The event's target state, or a state its FINISHED chain runs through, closes gates.</summary>
    private static bool LeadsToLock(Fsm fsm, FsmState state, FsmEvent fsmEvent)
    {
        string? target = NextState(state.Transitions.Concat(fsm.GlobalTransitions), fsmEvent);
        HashSet<string> seen = [];
        for (int depth = 0; depth < FinishedChainDepth && target is not null && seen.Add(target); depth++)
        {
            if (fsm.GetState(target) is not { } next)
            {
                return false;
            }
            if (ActionsOf(next).Any(ClosesGates))
            {
                return true;
            }
            target = NextState(next.Transitions, FsmEvent.Finished);
        }
        return false;
    }

    private static string? NextState(IEnumerable<FsmTransition> transitions, FsmEvent fsmEvent) =>
        transitions.FirstOrDefault(transition => transition.FsmEvent == fsmEvent)?.ToState;

    private static bool ClosesGates(FsmStateAction action)
    {
        return action.Enabled && action switch
        {
            SendEventByName send => send.sendEvent?.Value == GatesClose,
            CallMethodProper call => StartsBattle(call.behaviour.Value, call.methodName.Value),
            _ => false,
        };
    }

    private static bool StartsBattle(string behaviour, string method)
    {
        bool starts = method is nameof(BattleScene.StartBattle) or nameof(BattleScene.LockInBattle);
        return starts && behaviour == nameof(BattleScene);
    }

    private static FsmStateAction[] ActionsOf(FsmState state)
    {
        try
        {
            return state.Actions ?? [];
        }
        catch (Exception)
        {
            return [];
        }
    }

    private sealed class Held(string name, Func<bool> stillPending, Action start)
    {
        public string Name { get; } = name;

        public Func<bool> StillPending { get; } = stillPending;

        public Action Start { get; } = start;

        public string? Scene { get; set; }

        public Vector2 Spot { get; set; }

        /// <summary>The partner's hero started it (a relayed event); the wait is at its spot.</summary>
        public bool PartnerStarted { get; set; }
    }
}
