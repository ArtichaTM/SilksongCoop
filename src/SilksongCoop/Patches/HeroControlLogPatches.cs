using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GlobalEnums;
using HarmonyLib;
using HutongGames.PlayMaker;

namespace SilksongCoop.Patches;

/// <summary>
/// Finds who kept a frozen hero (marks 26/27): every call that takes or gives back the hero's control is
/// logged with the FSM state or C# method that made it and the flags it left.
/// </summary>
[HarmonyPatch(typeof(HeroController))]
internal static class HeroControlLogPatches
{
    private static readonly AccessTools.FieldRef<HeroController, HashSet<object>> InputBlockers =
        AccessTools.FieldRefAccess<HeroController, HashSet<object>>("inputBlockers");

    public static bool Blocks(HeroController hero, object blocker) => InputBlockers(hero).Contains(blocker);

    /// <summary>The control flags, for the log and the dev report.</summary>
    public static string Describe(HeroController hero)
    {
        IEnumerable<string> names = InputBlockers(hero).Select(blocker => blocker?.ToString() ?? "null");
        string blockers = string.Join(", ", names);
        return $"accepting={hero.acceptingInput} relinquished={hero.controlReqlinquished} "
            + $"locks={hero.HeroLockState} blockers=[{blockers}] dead={hero.cState.dead} "
            + $"transitioning={hero.cState.transitioning}";
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(HeroController.RelinquishControl))]
    private static void Relinquish(HeroController __instance) => Log(__instance, "RelinquishControl");

    [HarmonyPostfix]
    [HarmonyPatch(nameof(HeroController.RelinquishControlNotVelocity))]
    private static void RelinquishNotVelocity(HeroController __instance) =>
        Log(__instance, "RelinquishControlNotVelocity");

    [HarmonyPostfix]
    [HarmonyPatch(nameof(HeroController.RegainControl), [typeof(bool)])]
    private static void Regain(HeroController __instance) => Log(__instance, "RegainControl");

    [HarmonyPostfix]
    [HarmonyPatch(nameof(HeroController.AddLockStates))]
    private static void AddLocks(HeroController __instance, HeroLockStates lockStates) =>
        Log(__instance, $"AddLockStates({lockStates})");

    [HarmonyPostfix]
    [HarmonyPatch(nameof(HeroController.RemoveLockStates))]
    private static void RemoveLocks(HeroController __instance, HeroLockStates lockStates) =>
        Log(__instance, $"RemoveLockStates({lockStates})");

    [HarmonyPostfix]
    [HarmonyPatch(nameof(HeroController.AddInputBlocker))]
    private static void AddBlocker(HeroController __instance, object blocker) =>
        Log(__instance, $"AddInputBlocker({blocker})");

    [HarmonyPostfix]
    [HarmonyPatch(nameof(HeroController.RemoveInputBlocker))]
    private static void RemoveBlocker(HeroController __instance, object blocker) =>
        Log(__instance, $"RemoveInputBlocker({blocker})");

    private static void Log(HeroController hero, string call)
    {
        if (Coop.Current is { InCoop: true } coop)
        {
            coop.Log.LogInfo($"hero control: {call} by {Caller()}; {Describe(hero)}");
        }
    }

    /// <summary>The running FSM action, else the first C# method outside the hero and this patch.</summary>
    private static string Caller()
    {
        if (FsmExecutionStack.ExecutingFsm is { } fsm)
        {
            string owner = fsm.GameObject ? fsm.GameObject.name : "-";
            string action = FsmExecutionStack.ExecutingAction?.GetType().Name ?? "-";
            string state = FsmExecutionStack.ExecutingStateName;
            return $"{owner} [{fsm.Name}] {state} {action}";
        }
        StackFrame? frame = new StackTrace().GetFrames()?.Skip(1).FirstOrDefault(frame =>
            frame.GetMethod()?.DeclaringType is { } type
            && type != typeof(HeroController)
            && type != typeof(HeroControlLogPatches)
            && !type.FullName!.StartsWith("HarmonyLib"));
        return frame?.GetMethod() is { } method ? $"{method.DeclaringType!.Name}.{method.Name}" : "-";
    }
}
