using HarmonyLib;

namespace SilksongCoop.Patches;

/// <summary>D31: the session ends when quit is pressed, not 2.5 s of fade and a scene load later.</summary>
[HarmonyPatch(typeof(GameManager), nameof(GameManager.ReturnToMainMenu))]
internal static class QuitPatches
{
    private static void Prefix() => Coop.Current?.OnQuitToMenu();
}
