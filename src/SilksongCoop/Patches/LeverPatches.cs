using HarmonyLib;

namespace SilksongCoop.Patches;

/// <summary>A hit that pulled a lever (1.4); a lever already pulled takes hits without effect.</summary>
[HarmonyPatch(typeof(Lever_tk2d), nameof(Lever_tk2d.Hit))]
internal static class LeverPatches
{
    private static void Prefix(Lever_tk2d __instance, out bool __state) => __state = __instance.activated;

    private static void Postfix(Lever_tk2d __instance, bool __state)
    {
        if (!__state && __instance.activated && Coop.Current is { InCoop: true } coop)
        {
            coop.Levers.OnLocalPull(__instance);
        }
    }
}
