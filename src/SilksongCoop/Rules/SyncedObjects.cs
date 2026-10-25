using System.Collections.Generic;
using System.Linq;

namespace SilksongCoop.Rules;

/// <summary>
/// Room FSMs that <c>EnemySync.Classify</c> misses: a plain <c>Control</c> FSM without a persistent item.
/// </summary>
internal static class SyncedObjects
{
    /// <summary>Hazards a boss sets off (name prefix): the owner's random X is the one that falls.</summary>
    private static readonly (string Prefix, string Fsm)[] OwnerRun =
    [
        ("Mossbone Stalactite", "Control"),
    ];

    /// <summary>Either player sets them off: impacts from both count, the break writes PlayerData.</summary>
    private static readonly HashSet<(string Object, string Fsm)> EitherTriggers =
    [
        ("Fixer Bridge Bottom", "Control"),
    ];

    public static bool IsOwnerRun(string objectName, string fsm) =>
        OwnerRun.Any(entry => entry.Fsm == fsm && objectName.StartsWith(entry.Prefix));

    public static bool IsEitherTriggers(string objectName, string fsm) =>
        EitherTriggers.Contains((objectName, fsm));
}
