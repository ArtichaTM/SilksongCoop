using HarmonyLib;

namespace SilksongCoop.Patches;

/// <summary>Reports the local player's dialogue skips to the partner.</summary>
[HarmonyPatch(typeof(DialogueBox), nameof(DialogueBox.AdvanceConversation))]
internal static class DialoguePatches
{
    private static void Prefix(DialogueBox __instance)
    {
        if (Coop.Current is { InCoop: true } coop)
        {
            coop.Dialogue.OnLocalSkip(__instance);
        }
    }
}
