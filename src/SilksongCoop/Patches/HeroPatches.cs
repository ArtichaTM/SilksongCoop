using System.Collections;
using GlobalEnums;
using HarmonyLib;
using UnityEngine;

namespace SilksongCoop.Patches;

/// <summary>H1, H2, H5: death becomes "down", nothing hurts the downed, Bind on a cocoon revives.</summary>
[HarmonyPatch(typeof(HeroController))]
internal static class HeroPatches
{
    private static bool IsDown => Coop.Current?.Downed.IsDown == true;

    [HarmonyPrefix]
    [HarmonyPatch(nameof(HeroController.Die))]
    private static bool Die(
        HeroController __instance,
        bool nonLethal,
        bool frostDeath,
        ref IEnumerator? __result
    )
    {
        Game.DownedState? downed = Coop.Current?.Downed;
        if (downed?.TryGoDown(__instance, nonLethal, frostDeath, out IEnumerator? routine) != true)
        {
            return true;
        }
        __result = routine!;
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(
        nameof(HeroController.TakeDamage),
        [
            typeof(GameObject), typeof(CollisionSide), typeof(int),
            typeof(HazardType), typeof(DamagePropertyFlags),
        ]
    )]
    private static bool TakeDamage() => !IsDown;

    [HarmonyPrefix]
    [HarmonyPatch(nameof(HeroController.CheckDeathCatch))]
    private static bool CheckDeathCatch() => !IsDown;

    [HarmonyPrefix]
    [HarmonyPatch(nameof(HeroController.AddHealth))]
    private static bool AddHealth(HeroController __instance, int amount) =>
        Coop.Current?.Downed.OnAddHealth(__instance, amount) ?? true;

    [HarmonyPostfix]
    [HarmonyPatch(nameof(HeroController.CocoonBroken), [typeof(bool), typeof(bool)])]
    private static void CocoonBroken() => Coop.Current?.Downed.OnCocoonBroken();
}
