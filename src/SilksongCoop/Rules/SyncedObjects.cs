using System.Collections.Generic;
using System.Linq;

namespace SilksongCoop.Rules;

/// <summary>
/// Room FSMs that <c>EnemySync.Classify</c> misses: a plain <c>Control</c> FSM without a persistent item.
/// </summary>
internal static class SyncedObjects
{
    /// <summary>
    /// Owner-run objects without HP (name prefix): a boss's hazards (the owner's random X falls), boulders
    /// either hero walks under (3.32) and corpses that release a creature (it comes out where the owner's
    /// corpse landed, 2.37).
    /// </summary>
    private static readonly (string Prefix, string Fsm)[] OwnerRun =
    [
        ("Mossbone Stalactite", "Control"),
        ("Bone_Boulder", "Control"),
        ("Corpse Shellwood Goomba Flyer", "Custom Corpse"),
    ];

    /// <summary>Owner-run objects that go once per room visit, and the FSM bool set as they go.</summary>
    private static readonly (string Prefix, string Fsm, string Flag)[] OnceOnly =
    [
        ("Bone_Boulder", "Control", "Activated"),
    ];

    /// <summary>Either player sets them off: impacts from both count, the break writes PlayerData.</summary>
    private static readonly HashSet<(string Object, string Fsm)> EitherTriggers =
    [
        ("Fixer Bridge Bottom", "Control"),
    ];

    /// <summary>Each game runs them (name prefix); the state a trigger leads to is replayed (7.6).</summary>
    private static readonly StateEntry[] Replayed =
    [
        new("explode_wall", "Control", "Strike"),
        new("Pilgrim Trap Wire", "Control", "Snap"),
    ];

    /// <summary>FSM pickups (name prefix) and the state a take enters: one take per world (7.3).</summary>
    private static readonly StateEntry[] FsmPickups =
    [
        new("Mossberry Pickup", "Control", "PD Bool"),
        new("Silk Spool", "Control", "Set PD"),
        new("Ladybug Craft Pickup", "FSM", "Wait"),
    ];

    public static bool IsOwnerRun(string objectName, string fsm) =>
        OwnerRun.Any(entry => entry.Fsm == fsm && objectName.StartsWith(entry.Prefix));

    /// <summary>The FSM bool that is true once this object went, else null.</summary>
    public static string? SpentFlag(string objectName, string fsm) =>
        OnceOnly.FirstOrDefault(entry => entry.Fsm == fsm && objectName.StartsWith(entry.Prefix)).Flag;

    public static bool IsEitherTriggers(string objectName, string fsm) =>
        EitherTriggers.Contains((objectName, fsm));

    public static bool IsReplayed(string objectName, string fsm, string state) =>
        Replayed.Any(entry => entry.Matches(objectName, fsm, state));

    /// <summary>The pickup's name prefix when this state takes it, else null.</summary>
    public static string? FsmPickupTaken(string objectName, string fsm, string state) =>
        FsmPickups.FirstOrDefault(entry => entry.Matches(objectName, fsm, state))?.Prefix;

    public static bool IsFsmPickup(string objectName, string fsm, string prefix) =>
        FsmPickups.Any(entry => entry.Prefix == prefix && entry.Fsm == fsm && objectName.StartsWith(prefix));

    /// <summary>An FSM state on objects whose name starts with <see cref="Prefix"/>.</summary>
    private sealed record StateEntry(string Prefix, string Fsm, string State)
    {
        public bool Matches(string objectName, string fsm, string state) =>
            (Fsm, State) == (fsm, state) && objectName.StartsWith(Prefix);
    }
}
