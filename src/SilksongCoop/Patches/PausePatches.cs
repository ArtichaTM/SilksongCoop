using System.Collections;
using System.Linq;
using HarmonyLib;

namespace SilksongCoop.Patches;

/// <summary>D29: the pause sync reacts after the game froze or resumed time, and can hold back Esc.</summary>
[HarmonyPatch(typeof(GameManager), nameof(GameManager.SetPausedState))]
internal static class PausePatches
{
    /// <summary>Inside <c>SetIsInventoryOpen</c>, which pauses the game like the pause menu does.</summary>
    private static bool _inventory;

    /// <summary>In coop the inventory and the bench's equip screen leave time running.</summary>
    private static bool Prefix() => !_inventory || Coop.Current is not { InCoop: true };

    private static void Postfix(bool value)
    {
        if (!_inventory)
        {
            Coop.Current?.Pause.OnLocalPause(value);
        }
    }

    /// <summary>Every pause and unpause, from Esc or the menu's Continue, passes here.</summary>
    [HarmonyPatch(typeof(GameManager), nameof(GameManager.PauseGameToggle))]
    private static class Toggle
    {
        private static bool Prefix(GameManager __instance, ref IEnumerator __result)
        {
            if (Coop.Current?.Pause.AllowToggle(__instance) ?? true)
            {
                return true;
            }
            __result = Enumerable.Empty<object>().GetEnumerator();
            return false;
        }
    }

    [HarmonyPatch(typeof(GameManager), nameof(GameManager.SetIsInventoryOpen))]
    private static class Inventory
    {
        private static void Prefix() => _inventory = true;

        private static void Finalizer() => _inventory = false;
    }
}
