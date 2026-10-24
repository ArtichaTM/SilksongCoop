using HarmonyLib;

namespace SilksongCoop.Patches;

/// <summary>A pressure plate that just activated (1.5 a); each kind keeps its own flag.</summary>
[HarmonyPatch]
internal static class PlatePatches
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(TempPressurePlate), "Activate")]
    private static void TempBefore(TempPressurePlate __instance, out bool __state) =>
        __state = __instance.isActivated;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(TempPressurePlate), "Activate")]
    private static void TempAfter(TempPressurePlate __instance, bool __state)
    {
        if (!__state && __instance.isActivated)
        {
            Coop.Current?.Plates.OnLocalPress(__instance);
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(PersistentPressurePlate), "Activate")]
    private static void PersistentBefore(PersistentPressurePlate __instance, out bool __state) =>
        __state = __instance.activated;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PersistentPressurePlate), "Activate")]
    private static void PersistentAfter(PersistentPressurePlate __instance, bool __state)
    {
        if (!__state)
        {
            Coop.Current?.Plates.OnLocalPress(__instance);
        }
    }
}
