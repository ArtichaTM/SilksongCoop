using HutongGames.PlayMaker;
using SilksongCoop.Net;
using UnityEngine;

namespace SilksongCoop.Game;

/// <summary>
/// Memories with two players (3.1 a): touching the orb starts its sequence in both games, and each game
/// leaves by its own ending instead of a pull, so both run the whole reward. Both games grant the same Silk
/// Heart, so meanwhile the friend takes the host's growing numbers instead of adding its own.
/// </summary>
internal sealed class MemorySync : Module
{
    private const string OrbPrefix = "memory_orb";
    private const string OrbFsm = "Control";
    private const string IdleState = "Idle";
    private const string TakeState = "Take Control";
    private const string TouchEvent = "TOUCH";
    private const float GrantGrace = 3f;

    private string? _orbScene;
    private float _grantUntil;
    private bool _applying;

    public MemorySync(Coop coop)
        : base(coop) => coop.Rooms.Entered += OnEntered;

    /// <summary>Friend side: a memory reward runs here and in the host's game; the host's counts.</summary>
    public bool HostGrants => _orbScene is not null || Time.unscaledTime < _grantUntil;

    public override void Register(Session session) => session.On(MessageType.MemoryOrbTaken, OnOrbTaken);

    public override void Dispose() => Coop.Rooms.Entered -= OnEntered;

    /// <summary>Called before any FSM switches state.</summary>
    public void OnSwitch(Fsm fsm, FsmState toState)
    {
        if (!IsOrb(fsm) || fsm.ActiveStateName != IdleState || toState.Name != TakeState)
        {
            return;
        }
        _orbScene = Coop.Rooms.Current;
        if (!_applying && Coop.Rooms.PartnerHere)
        {
            Session?.Send(MessageType.MemoryOrbTaken, w => w.String(fsm.GameObject.name));
        }
    }

    /// <summary>Leaving the memory whose orb was taken while the partner is still in it: no pull.</summary>
    public bool EndsOnItsOwn(string? scene) => _orbScene is not null && scene == _orbScene && PartnerInMemory;

    private bool PartnerInMemory => Coop.Partner.Scene == _orbScene;

    private void OnEntered(string scene)
    {
        if (_orbScene is not null && scene != _orbScene)
        {
            _orbScene = null;
            _grantUntil = Time.unscaledTime + GrantGrace;
        }
    }

    private void OnOrbTaken(PacketReader reader)
    {
        string name = reader.String();
        if (!Coop.Rooms.PartnerHere)
        {
            return;
        }
        foreach (PlayMakerFSM component in Object.FindObjectsByType<PlayMakerFSM>(FindObjectsSortMode.None))
        {
            bool waiting = IsOrb(component.Fsm) && component.ActiveStateName == IdleState;
            if (waiting && component.gameObject.name == name)
            {
                Touch(component);
                return;
            }
        }
    }

    private void Touch(PlayMakerFSM orb)
    {
        _applying = true;
        try
        {
            orb.SendEvent(TouchEvent);
        }
        finally
        {
            _applying = false;
        }
    }

    private bool IsOrb(Fsm fsm)
    {
        GameObject go = fsm.GameObject;
        return fsm.Name == OrbFsm && go && go.name.StartsWith(OrbPrefix) && Coop.Rooms.Holds(go.scene);
    }
}
