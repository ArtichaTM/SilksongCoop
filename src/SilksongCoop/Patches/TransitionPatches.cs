using HarmonyLib;
using UnityEngine;

namespace SilksongCoop.Patches;

/// <summary>H6, H7: the tether door on gates and doors, and pulls on every scene change.</summary>
[HarmonyPatch]
internal static class TransitionPatches
{
    private static Game.Pulls? Pulls => Coop.Current?.Pulls;

    [HarmonyPrefix]
    [HarmonyPatch(typeof(TransitionPoint), nameof(TransitionPoint.TryDoTransition))]
    private static bool WalkThrough(TransitionPoint __instance, Collider2D heroCollider) =>
        Pulls?.AllowGate(__instance, heroCollider) ?? true;

    [HarmonyPrefix]
    [HarmonyPatch(typeof(TransitionPoint), nameof(TransitionPoint.Interact))]
    private static bool Door(TransitionPoint __instance) => Pulls?.AllowGate(__instance, null) ?? true;

    [HarmonyPrefix]
    [HarmonyPatch(typeof(GameManager), nameof(GameManager.BeginSceneTransition))]
    private static void SceneChange(GameManager.SceneLoadInfo info) => Pulls?.OnSceneTransition(info);
}
