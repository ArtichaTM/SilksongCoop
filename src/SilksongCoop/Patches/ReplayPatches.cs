using HarmonyLib;

namespace SilksongCoop.Patches;

/// <summary>A cut platform's final cut (7.1): the partner's copy drops too.</summary>
[HarmonyPatch(typeof(SuspendedPlatformCut), nameof(SuspendedPlatformCut.Cut))]
internal static class ReplayPatches
{
    private static void Postfix(SuspendedPlatformCut __instance)
    {
        if (Coop.Current is { InCoop: true } coop)
        {
            coop.Replays.OnLocalCut(__instance);
        }
    }
}
