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
/// While an arena waits, its gates close for the early player (3.5): no enemies, no music, no way past.
/// The late player's arrival starts it for both; a session that ends first starts it alone.
/// </summary>
internal sealed class PartnerWaits : Module
{
    private const float NearDistance = 10f;
    private const float HintInterval = 2.5f;
    private const int FinishedChainDepth = 8;
    private const string GatesClose = "BG CLOSE";

    private readonly Dictionary<(Fsm Fsm, string State, string Event), Start> _starts = [];
    private Held? _held;
    private float _nextHint;
    private bool _replaying;

    public PartnerWaits(Coop coop)
        : base(coop) => coop.Rooms.Entered += OnEntered;

    /// <summary>
    /// Gates closing for a wait (3.5): they close in this game only, so the late player can still walk in,
    /// and nobody is pulled.
    /// </summary>
    public static bool Locking { get; private set; }

    public override void Register(Session session) => session.On(MessageType.PartnerWait, OnPartnerWait);

    public override void Update()
    {
        if (_held is not { } held)
        {
            return;
        }
        if (!held.StillPending() || Coop.Rooms.Current != held.Scene)
        {
            Release();
            return;
        }
        if (!Coop.InCoop)
        {
            StartAlone();
            return;
        }
        if (BothAt(held.Spot))
        {
            Release();
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

    public override void OnLeft() => StartAlone();

    /// <summary>Nobody is coming: the shut arena starts for the player inside it.</summary>
    private void StartAlone()
    {
        if (_held is not { } held)
        {
            return;
        }
        Release();
        if (held.Locked && held.StillPending() && Coop.Rooms.Current == held.Scene)
        {
            Coop.Log.LogInfo($"partner gone, starting {held.Name} alone");
            Replay(held);
        }
    }

    public override void Dispose() => Coop.Rooms.Entered -= OnEntered;

    /// <summary>The wait is over: gates it closed sync again (the start that follows closes both).</summary>
    private void Release()
    {
        _held = null;
        Coop.Enemies.ReleaseLocalGates();
    }

    /// <summary>Before an unmirrored FSM takes an event: true holds it back until both are here.</summary>
    public bool HoldsEvent(Fsm fsm, FsmEvent? fsmEvent)
    {
        bool plain = fsmEvent is null || fsmEvent == FsmEvent.Finished || !fsm.GameObject;
        if (_replaying || plain || fsm.ActiveState is not { } state || !MustWait())
        {
            return false;
        }
        Start start = StartOf(fsm, state, fsmEvent!);
        if (!start.Waits)
        {
            return false;
        }
        string stateName = state.Name;
        string name = $"{fsm.GameObject!.name} [{fsm.Name}] {stateName} -{fsmEvent!.Name}->";
        Held held = new(name, () => fsm.ActiveStateName == stateName, () => fsm.Event(fsmEvent), start.Lock);
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
        Hold(new Held(battle.name, pending, battle.StartBattle, battle.LockInBattle), false);
        return true;
    }

    /// <summary>Whether taking this event starts what the partner must not miss, and its gates.</summary>
    private Start StartOf(Fsm fsm, FsmState state, FsmEvent fsmEvent)
    {
        (Fsm, string, string) key = (fsm, state.Name, fsmEvent.Name);
        if (!_starts.TryGetValue(key, out Start start))
        {
            FsmStateAction? closer = GateCloser(fsm, state, fsmEvent);
            bool listed = WaitPoints.Waits(fsm.GameObject.name, fsm.Name, state.Name, fsmEvent.Name);
            start = new Start(listed || closer is not null, closer is null ? null : () => CloseGates(closer));
            _starts[key] = start;
        }
        return start;
    }

    /// <summary>Coop, this player up, and the partner not near or down (a downed one is summoned).</summary>
    private bool MustWait()
    {
        if (!Coop.InCoop || Coop.Rooms.Current is null || Coop.Downed.IsDown)
        {
            return false;
        }
        bool near = GameRefs.Hero is { } hero && PartnerNear(hero.transform.position);
        return Coop.Partner.Downed || !near;
    }

    private bool BothAt(Vector2 spot)
    {
        bool partner = !Coop.Partner.Downed && PartnerNear(spot);
        return partner && GameRefs.Hero is { } hero && Near(hero.transform.position, spot);
    }

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
        if (partnerStarted)
        {
            return;
        }
        if (held.Lock is { } shut)
        {
            // 3.5: the early player is shut in; the late one's own start closes its gates later.
            Locking = true;
            try
            {
                shut();
            }
            finally
            {
                Locking = false;
            }
            held.Locked = true;
            Coop.Log.LogInfo($"gates closed while {held.Name} waits");
        }
        if (Coop.Partner.Downed)
        {
            Coop.Downed.SummonPartner();
        }
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
        Release();
        _starts.Clear();
    }

    /// <summary>The action that closes gates in the event's target state or its FINISHED chain.</summary>
    private static FsmStateAction? GateCloser(Fsm fsm, FsmState state, FsmEvent fsmEvent)
    {
        string? target = NextState(state.Transitions.Concat(fsm.GlobalTransitions), fsmEvent);
        HashSet<string> seen = [];
        for (int depth = 0; depth < FinishedChainDepth && target is not null && seen.Add(target); depth++)
        {
            if (fsm.GetState(target) is not { } next)
            {
                return null;
            }
            if (ActionsOf(next).FirstOrDefault(ClosesGates) is { } closer)
            {
                return closer;
            }
            target = NextState(next.Transitions, FsmEvent.Finished);
        }
        return null;
    }

    /// <summary>Only the gate part of the start: the same "BG CLOSE" send, or the battle's lock.</summary>
    private static void CloseGates(FsmStateAction closer)
    {
        switch (closer)
        {
            case SendEventByName send when send.eventTarget?.target != FsmEventTarget.EventTarget.Self:
                send.Fsm.Event(send.eventTarget, GatesClose);
                break;
            case CallMethodProper call when call.Fsm.GetOwnerDefaultTarget(call.gameObject) is { } go:
                if (go.TryGetComponent(out BattleScene battle))
                {
                    battle.LockInBattle();
                }
                break;
        }
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

    private readonly record struct Start(bool Waits, Action? Lock);

    private sealed class Held(string name, Func<bool> stillPending, Action start, Action? close)
    {
        public string Name { get; } = name;

        public Func<bool> StillPending { get; } = stillPending;

        public Action Start { get; } = start;

        /// <summary>Closes the gates without starting the fight (3.5); null for a cutscene.</summary>
        public Action? Lock { get; } = close;

        /// <summary>The early player is shut in: the start runs even if the partner never comes.</summary>
        public bool Locked { get; set; }

        public string? Scene { get; set; }

        public Vector2 Spot { get; set; }

        /// <summary>The partner's hero started it (a relayed event); the wait is at its spot.</summary>
        public bool PartnerStarted { get; set; }
    }
}
