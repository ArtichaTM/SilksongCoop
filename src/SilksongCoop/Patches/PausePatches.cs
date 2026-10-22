using HarmonyLib;

namespace SilksongCoop.Patches;

/// <summary>D29: lets the pause sync react after the game froze or resumed time.</summary>
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

    [HarmonyPatch(typeof(GameManager), nameof(GameManager.SetIsInventoryOpen))]
    private static class Inventory
    {
        private static void Prefix() => _inventory = true;

        private static void Finalizer() => _inventory = false;
    }
}
