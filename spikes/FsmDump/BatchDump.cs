using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HarmonyLib;
using HutongGames.PlayMaker;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace FsmDump;

/// <summary>
/// Loads every addressable scene additively, dumps its FSMs and objects, unloads it.
/// FSMs of loaded scenes never start (Fsm.Start is skipped) and roots are deactivated in sceneLoaded.
/// A scene with an existing .fsm.txt or listed in _stuck.txt is skipped, so a run resumes after a restart.
/// </summary>
internal static class BatchDump
{
    private const string Prefix = "Scenes/";
    private const float LoadTimeout = 60f;

    private static Scene _home;

    internal static string OutputDir { get; } = Path.Combine(FsmDumpPlugin.OutputDir, "all");

    public static bool IsRunning { get; private set; }

    public static string Progress { get; private set; } = "";

    private static string StuckPath => Path.Combine(OutputDir, "_stuck.txt");

    public static IEnumerator Run()
    {
        IsRunning = true;
        _home = SceneManager.GetActiveScene();
        Directory.CreateDirectory(OutputDir);
        SceneManager.sceneLoaded += Freeze;
        try
        {
            string[] names = SceneNames();
            File.WriteAllLines(Path.Combine(OutputDir, "_scenes.txt"), names);
            HashSet<string> stuck = File.Exists(StuckPath) ? [.. File.ReadAllLines(StuckPath)] : [];
            for (int index = 0; index < names.Length; index++)
            {
                string name = names[index];
                if (File.Exists(FsmPath(name)) || stuck.Contains(name))
                {
                    continue;
                }
                Progress = $"{index + 1}/{names.Length} {name}";
                bool[] loaded = [false];
                yield return DumpOne(name, loaded);
                if (!loaded[0])
                {
                    File.AppendAllText(StuckPath, name + "\n");
                    Progress = $"STUCK on {name}: restart the game and press F8 again (it will be skipped)";
                    yield break;
                }
                if (index % 20 == 19)
                {
                    yield return Resources.UnloadUnusedAssets();
                }
            }
            Progress = $"done, {names.Length} scenes";
        }
        finally
        {
            SceneManager.sceneLoaded -= Freeze;
            IsRunning = false;
        }
    }

    /// <summary>True for objects of a batch-loaded scene: not the player's scene, not persistent.</summary>
    public static bool IsForeign(GameObject? go) =>
        IsRunning && go != null && go.scene != _home && go.scene.name != "DontDestroyOnLoad";

    private static IEnumerator DumpOne(string name, bool[] loaded)
    {
        string address = Prefix + name;
        var handle = Addressables.LoadSceneAsync(address, LoadSceneMode.Additive);
        float deadline = Time.realtimeSinceStartup + LoadTimeout;
        while (!handle.IsDone)
        {
            if (Time.realtimeSinceStartup > deadline)
            {
                Fail(name, $"load timed out after {LoadTimeout} s");
                yield break;
            }
            yield return null;
        }
        loaded[0] = true;
        if (handle.Status != AsyncOperationStatus.Succeeded)
        {
            Fail(name, $"load {handle.Status}: {handle.OperationException?.Message}");
            yield break;
        }
        yield return null;
        try
        {
            Scene scene = handle.Result.Scene;
            ObjectWriter.DumpScene(scene, Path.Combine(OutputDir, $"{name}.objects.txt"));
            FsmWriter.DumpScene(scene, FsmPath(name));
        }
        catch (Exception exception)
        {
            Fail(name, $"dump {exception}");
        }
        yield return Addressables.UnloadSceneAsync(handle);
    }

    private static string[] SceneNames()
    {
        IEnumerable<string> keys = Addressables.ResourceLocators
            .SelectMany(locator => locator.Keys.OfType<string>().Where(key => IsScene(locator, key)))
            .Select(key => key.Substring(Prefix.Length));
        return keys.Distinct().OrderBy(name => name, StringComparer.Ordinal).ToArray();
    }

    private static bool IsScene(IResourceLocator locator, string key) =>
        key.StartsWith(Prefix, StringComparison.Ordinal) && locator.Locate(key, typeof(SceneInstance), out _);

    private static void Freeze(Scene scene, LoadSceneMode mode)
    {
        if (scene == _home)
        {
            return;
        }
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            root.SetActive(false);
        }
    }

    private static string FsmPath(string name) => Path.Combine(OutputDir, $"{name}.fsm.txt");

    private static void Fail(string name, string reason)
    {
        Debug.LogWarning($"[FsmDump] {name}: {reason}");
        File.AppendAllText(Path.Combine(OutputDir, "_failed.txt"), $"{name}\t{reason}\n");
    }

    [HarmonyPatch(typeof(Fsm), nameof(Fsm.Start))]
    private static class FsmStartPatch
    {
        private static bool Prefix(Fsm __instance) => !IsForeign(__instance.GameObject);
    }
}
