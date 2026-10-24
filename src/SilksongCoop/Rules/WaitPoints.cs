using System.Collections.Generic;

namespace SilksongCoop.Rules;

/// <summary>
/// Solo takeovers that wait for the partner (4.2 a) although no arena gate closes: an FSM on an object of
/// this name, in this state, taking this event. Arenas need no entry; they are found by their gates.
/// </summary>
internal static class WaitPoints
{
    private static readonly HashSet<(string Object, string Fsm, string State, string Event)> Points =
    [
        ("Skull King", "Behaviour", "State Check", "WAKE"),
        ("Skull King", "Behaviour", "Invade Ready", "BENCHREST"),
        ("Steel Servant Scene", "Control", "Idle", "ENTER"),
        ("Garmond Fighter", "Control", "Dormant", "GARMOND START"),
        ("Churchkeeper Intro Scene", "Control", "Idle", "WOUND"),
        ("Mapper NPC", "Ring Shout", "Ready", "ALERT"),
        ("Funeral Leader", "Control", "Idle", "INTERACT"),
        ("Shrine Weaver Ability", "Inspection", "Idle", "INTERACT"),
        ("Caravan", "Quest End Sequence", "Wait", "QUEST END TRAVEL"),
    ];

    public static bool Waits(string objectName, string fsm, string state, string fsmEvent) =>
        Points.Contains((objectName, fsm, state, fsmEvent));
}
