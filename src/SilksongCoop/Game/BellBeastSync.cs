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

    /// <summary>Called but not shown yet: the appearance may start from here.</summary>
    private static readonly HashSet<string> NotShown =
    [
        "Wait For Call", "Wait For Enter", "Check Call", "Needolin Wait", "Hero Call", "Appear Delay",
    ];

    /// <summary>Shown and waiting for a ride: the trip starts with the hero's leap, as if chosen.</summary>
    private static readonly HashSet<string> Waiting = ["Idle", "Activate", "End Travel Interaction"];

    private bool _applying;
    private float _tripUntil = float.NegativeInfinity;

    public BellBeastSync(Coop coop)
        : base(coop) => coop.Rooms.Entered += OnEntered;

    /// <summary>Both games are on one trip: its scene changes are not pulls.</summary>
    public bool TravelsTogether => Time.unscaledTime < _tripUntil;

    public override void Register(Session session)
    {
        session.On(MessageType.BeastAppear, OnAppear);
        session.On(MessageType.BeastTrip, OnTrip);
        session.On(MessageType.BeastTripJoined, _ => _tripUntil = Time.unscaledTime + TripWindow);
    }

    public override void OnLeft() => _tripUntil = float.NegativeInfinity;

    public override void Dispose() => Coop.Rooms.Entered -= OnEntered;

    /// <summary>Called before any FSM switches state.</summary>
    public void OnSwitch(Fsm fsm, FsmState toState)
    {
        if (_applying || !IsBeast(fsm) || !Coop.Rooms.PartnerHere)
        {
            return;
        }
        int room = Coop.Rooms.CurrentHash;
        if (toState.Name == AppearState)
        {
            Session?.Send(MessageType.BeastAppear, w => w.Int(room));
        }
        else if (toState.Name == TripState && Destination(fsm) is { } destination)
        {
            string scene = destination.Value;
            Session?.Send(MessageType.BeastTrip, w => w.Int(room).String(scene));
        }
    }

    private static bool IsBeast(Fsm fsm) => fsm.Name == BeastFsm && fsm.GameObject.name == BeastObject;

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
        _tripUntil = Time.unscaledTime + TripWindow;
        Session?.Send(MessageType.BeastTripJoined, _ => { });
        SetState(beast, Waiting.Contains(beast.ActiveStateName) ? TripState : QuietTripState);
    }

    private void SetState(PlayMakerFSM beast, string state)
    {
        _applying = true;
        try
        {
            beast.SetState(state);
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
