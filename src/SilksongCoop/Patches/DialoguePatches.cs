using HarmonyLib;
using SilksongCoop.Game;

namespace SilksongCoop.Patches;

/// <summary>Dialogue in coop: skips go to the partner, and the partner's talk never holds our hero.</summary>
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

    [HarmonyPrefix]
    [HarmonyPatch(typeof(HeroTalkAnimation), nameof(HeroTalkAnimation.EnterConversation))]
    private static bool TalkPose(NPCControlBase sourceNpc) => Sync?.BlocksLock(sourceNpc, "talk") != true;

    [HarmonyPrefix]
    [HarmonyPatch(typeof(HeroController), nameof(HeroController.RelinquishControl))]
    private static bool Relinquish() => Sync?.BlocksLock(DialogueSync.RunningNpc(), "relinquish") != true;

    [HarmonyPrefix]
    [HarmonyPatch(typeof(HeroController), nameof(HeroController.AddInputBlocker))]
    private static bool InputBlock() => Sync?.BlocksLock(DialogueSync.RunningNpc(), "input block") != true;
}
