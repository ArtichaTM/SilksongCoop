using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using HutongGames.PlayMaker;
using UnityEngine;

namespace FsmDump;

/// <summary>Always-on log of FSM state switches plus hooked C# calls (heal, bench, scene change).</summary>
internal static class Trace
{
    private static StreamWriter? _writer;
    private static int _marks;

    public static bool IsRecording => _writer is not null;

    public static void Open(string path)
    {
        _writer = new StreamWriter(path) { AutoFlush = true };
        _marks = 0;
    }

    public static void Close()
    {
        _writer?.Dispose();
        _writer = null;
    }

    /// <summary>Writes a numbered marker so a probe can be found in the file; returns its number.</summary>
    public static int Mark()
    {
        _marks++;
        Line($"===== MARK {_marks} =====");
        return _marks;
    }

    private static void Line(string text) => _writer?.WriteLine($"{Time.frameCount} {Time.time:F2} {text}");

    [HarmonyPatch(typeof(Fsm), nameof(Fsm.SwitchState))]
    private static class SwitchStatePatch
    {
        private static void Prefix(Fsm __instance, FsmState toState)
        {
            if (_writer is null || toState is null)
            {
                return;
            }
            string via = __instance.LastTransition?.EventName ?? "-";
            string where = FsmWriter.PathOf(__instance.GameObject);
            Line($"FSM {where} [{__instance.Name}] {__instance.ActiveStateName} -({via})-> {toState.Name}");
        }
    }

    [HarmonyPatch(typeof(PlayerData), nameof(PlayerData.AddHealth))]
    private static class AddHealthPatch
    {
        private static void Prefix(PlayerData __instance, int amount) =>
            Line($"CALL PlayerData.AddHealth({amount}) health={__instance.health}");
    }

    [HarmonyPatch]
    private static class BenchRespawnPatch
    {
        private static IEnumerable<MethodBase> TargetMethods()
        {
            return typeof(PlayerData).GetMethods()
                .Where(method => method.Name == nameof(PlayerData.SetBenchRespawn));
        }

        private static void Prefix(object[] __args) =>
            Line($"CALL PlayerData.SetBenchRespawn({string.Join(", ", __args)})");
    }

    [HarmonyPatch(typeof(GameManager), nameof(GameManager.BeginSceneTransition))]
    private static class SceneTransitionPatch
    {
        private static void Prefix(GameManager.SceneLoadInfo info) =>
            Line($"CALL BeginSceneTransition scene={info.SceneName} gate={info.EntryGateName}");
    }

    /// <summary>Set by the boss-title hook; the plugin shows it on screen.</summary>
    public static string? LastBossTitle { get; set; }

    [HarmonyPatch(typeof(HutongGames.PlayMaker.Actions.DisplayBossTitle), "OnEnter")]
    private static class BossTitlePatch
    {
        private static void Prefix(HutongGames.PlayMaker.Actions.DisplayBossTitle __instance)
        {
            string where = FsmWriter.PathOf(__instance.Fsm.GameObject);
            string title = __instance.bossTitle.Value;
            LastBossTitle = title;
            string state = $"[{__instance.Fsm.Name}] state {__instance.State.Name}";
            Line($"BOSS TITLE \"{title}\" from {where} {state}");
        }
    }

    [HarmonyPatch(typeof(GameManager), nameof(GameManager.SaveGame), typeof(System.Action<bool>))]
    private static class SaveGamePatch
    {
        private static void Prefix() => Line("CALL GameManager.SaveGame");
    }
}
