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

/// <summary>A crank platform's climb reached its end (not a save load): the partner's copy too.</summary>
[HarmonyPatch(typeof(CrankPlat), nameof(CrankPlat.SetComplete))]
internal static class CrankPatches
{
    private static void Postfix(CrankPlat __instance)
    {
        if (__instance.hitRoutine != null && Coop.Current is { InCoop: true } coop)
        {
            coop.Replays.OnLocalCrank(__instance);
        }
    }
}
