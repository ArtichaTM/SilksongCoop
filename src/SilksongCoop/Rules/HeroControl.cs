using System.Collections.Generic;

namespace SilksongCoop.Rules;

/// <summary>Hero calls that mark an FSM as a cutscene of the local hero; each game runs its own.</summary>
internal static class HeroControl
{
    private static readonly HashSet<string> TakesOrGivesControl =
    [
        "RelinquishControl",
        "RelinquishControlNotVelocity",
        "RegainControl",
        "StopAnimationControl",
        "StartAnimationControl",
        "StartAnimationControlToIdle",
        "StartAnimationControlToIdleForcePlay",
    ];

    public static bool IsControlCall(string? method) =>
        method is not null && TakesOrGivesControl.Contains(method);
}
