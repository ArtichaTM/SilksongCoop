using System.Collections.Generic;
using System.Linq;
using HutongGames.PlayMaker;
using SilksongCoop.Net;
using UnityEngine;

namespace SilksongCoop.Game;

/// <summary>
/// Bellway travel with two players in the station (1.1, design 3.2 a). Either player's call makes the beast
/// appear in both games; a trip one player sets off starts the partner's beast on the same trip, and each
/// game then plays its own cinematic and arrival instead of a pull. A partner elsewhere is pulled as usual.
/// The beast freed from the centipede (5.18) sings in both games once either player talks to it, and each
/// game travels on its own once its player has played the melody back.
/// </summary>
internal sealed class BellBeastSync : Module
{
    private const string BeastObject = "Bone Beast NPC";
    private const string BeastFsm = "Interaction";
    private const string AppearState = "Start Shake";
    private const string TripState = "Fade";
    private const string QuietTripState = "Time Passes";
    private const string DestinationVariable = "To Scene";
    private const string TravelScene = "Cinematic_Stag_travel";
    private const float TripWindow = 60f;
    private const string FreedObject = "Bell Beast DefeatedCentipede NPC";
    private const string FreedFsm = "Control";
    private const string SingState = "Hornet Speak";

    // The melody prompt and the item message wait for each player's input.
    private const float SingWindow = 300f;

    /// <summary>Called but not shown yet: the appearance may start from here.</summary>
    private static readonly HashSet<string> NotShown =
    [
        "Wait For Call", "Wait For Enter", "Check Call", "Needolin Wait", "Hero Call", "Appear Delay",
    ];

    /// <summary>Shown and waiting for a ride: the trip starts with the hero's leap, as if chosen.</summary>
    private static readonly HashSet<string> Waiting = ["Idle", "Activate", "End Travel Interaction"];

    /// <summary>The freed beast before anyone talked to it.</summary>
    private static readonly HashSet<string> FreedIdle =
    [
        "NPC Pause", "Start NPC", "Idle L", "Idle R", "Turn L", "Turn R", "Purr",
        "Tink React L", "Tink L End", "Tink React R", "Tink R End",
    ];

    private bool _applying;
    private float _tripUntil = float.NegativeInfinity;
    // How long the partner's ack keeps this game on the trip: the last trip or song sent.
    private float _ackWindow = TripWindow;

    public BellBeastSync(Coop coop)
        : base(coop) => coop.Rooms.Entered += OnEntered;

    /// <summary>Both games are on one trip: its scene changes are not pulls.</summary>
    public bool TravelsTogether => Time.unscaledTime < _tripUntil;

    public override void Register(Session session)
    {
        session.On(MessageType.BeastAppear, OnAppear);
        session.On(MessageType.BeastTrip, OnTrip);
        session.On(MessageType.BeastSing, OnSing);
        session.On(MessageType.BeastTripJoined, _ => _tripUntil = Time.unscaledTime + _ackWindow);
    }

    public override void OnLeft() => _tripUntil = float.NegativeInfinity;

    public override void Dispose() => Coop.Rooms.Entered -= OnEntered;

    /// <summary>Called before any FSM switches state.</summary>
    public void OnSwitch(Fsm fsm, FsmState toState)
    {
        if (_applying || !Coop.Rooms.PartnerHere || !fsm.GameObject)
        {
            return;
        }
        int room = Coop.Rooms.CurrentHash;
        if (IsFreed(fsm) && toState.Name == SingState)
        {
            _ackWindow = SingWindow;
            Session?.Send(MessageType.BeastSing, w => w.Int(room));
        }
        else if (!IsBeast(fsm))
        {
            return;
        }
        else if (toState.Name == AppearState)
        {
            Session?.Send(MessageType.BeastAppear, w => w.Int(room));
        }
        else if (toState.Name == TripState && Destination(fsm) is { } destination)
        {
            _ackWindow = TripWindow;
            string scene = destination.Value;
            Session?.Send(MessageType.BeastTrip, w => w.Int(room).String(scene));
        }
    }

    private static bool IsBeast(Fsm fsm) => fsm.Name == BeastFsm && fsm.GameObject.name == BeastObject;

    private static bool IsFreed(Fsm fsm) => fsm.Name == FreedFsm && fsm.GameObject.name == FreedObject;

    /// <summary>Also for unstarted FSMs, whose <c>Fsm.GameObject</c> is not set yet.</summary>
    private static bool IsBeast(PlayMakerFSM fsm) => fsm.FsmName == BeastFsm && fsm.name == BeastObject;

    private static FsmString? Destination(Fsm fsm) => fsm.Variables.GetFsmString(DestinationVariable);

    private void OnAppear(PacketReader reader)
    {
        bool forThisRoom = reader.Int() == Coop.Rooms.CurrentHash;
        if (forThisRoom && FindBeast() is { } beast && NotShown.Contains(beast.ActiveStateName))
        {
            SetState(beast, AppearState);
        }
    }

    private void OnTrip(PacketReader reader)
    {
        bool forThisRoom = reader.Int() == Coop.Rooms.CurrentHash;
        string scene = reader.String();
        if (!forThisRoom || FindBeast() is not { } beast || Destination(beast.Fsm) is not { } to)
        {
            return;
        }
        to.Value = scene;
        JoinTrip(TripWindow);
        SetState(beast, Waiting.Contains(beast.ActiveStateName) ? TripState : QuietTripState);
    }

    private void OnSing(PacketReader reader)
    {
        bool forThisRoom = reader.Int() == Coop.Rooms.CurrentHash;
        PlayMakerFSM? freed = Resources.FindObjectsOfTypeAll<PlayMakerFSM>()
            .FirstOrDefault(fsm => fsm.FsmName == FreedFsm && fsm.name == FreedObject
                && Coop.Rooms.Holds(fsm.gameObject.scene));
        if (!forThisRoom || freed is null || !FreedIdle.Contains(freed.ActiveStateName))
        {
            return;
        }
        JoinTrip(SingWindow);
        SetState(freed, SingState);
    }

    /// <summary>This game goes too: neither game's scene change pulls the other.</summary>
    private void JoinTrip(float window)
    {
        _tripUntil = Time.unscaledTime + window;
        Session?.Send(MessageType.BeastTripJoined, _ => { });
    }

    private void SetState(PlayMakerFSM fsm, string state)
    {
        _applying = true;
        try
        {
            fsm.SetState(state);
        }
        finally
        {
            _applying = false;
        }
    }

    /// <summary>The trip is over once a real room is reached; the cinematic is still on the way.</summary>
    private void OnEntered(string scene)
    {
        if (scene != TravelScene)
        {
            _tripUntil = float.NegativeInfinity;
        }
    }

    private PlayMakerFSM? FindBeast()
    {
        return Resources.FindObjectsOfTypeAll<PlayMakerFSM>()
            .FirstOrDefault(fsm => IsBeast(fsm) && Coop.Rooms.Holds(fsm.gameObject.scene));
    }
}
