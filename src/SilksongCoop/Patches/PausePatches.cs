using HarmonyLib;

namespace SilksongCoop.Patches;

/// <summary>D29: lets the pause sync react after the game froze or resumed time.</summary>
[HarmonyPatch(typeof(GameManager), nameof(GameManager.SetPausedState))]
internal static class PausePatches
{
    private static void Postfix(bool value) => Coop.Current?.Pause.OnLocalPause(value);
}
