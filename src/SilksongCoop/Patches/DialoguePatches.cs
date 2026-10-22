using HarmonyLib;
using SilksongCoop.Game;

namespace SilksongCoop.Patches;

/// <summary>Dialogue in coop: cutscene skips go to the partner; a player's own NPC talk is tracked.</summary>
[HarmonyPatch]
internal static class DialoguePatches
{
    private static DialogueSync? Sync => Coop.Current is { InCoop: true } coop ? coop.Dialogue : null;

    [HarmonyPrefix]
    [HarmonyPatch(typeof(DialogueBox), nameof(DialogueBox.AdvanceConversation))]
    private static void Advance(DialogueBox __instance) => Sync?.OnLocalSkip(__instance);

    [HarmonyPrefix]
    [HarmonyPatch(typeof(NPCControlBase), nameof(NPCControlBase.Interact))]
    private static void Interact(NPCControlBase __instance) => Sync?.OnLocalInteract(__instance);

    [HarmonyPostfix]
    [HarmonyPatch(typeof(NPCControlBase), nameof(NPCControlBase.EndDialogue))]
    private static void Ended(NPCControlBase __instance) => Sync?.OnDialogueEnded(__instance);
}
