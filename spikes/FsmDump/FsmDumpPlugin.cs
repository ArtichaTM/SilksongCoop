using System;
using System.IO;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FsmDump;

/// <summary>Records FSM traces with markers and dumps FSMs; an on-screen box shows the state.</summary>
[BepInPlugin("articha.fsmdump", "FsmDump", "0.1.0")]
public sealed class FsmDumpPlugin : BaseUnityPlugin
{
    private ConfigEntry<KeyCode> _dumpKey = null!;
    private ConfigEntry<KeyCode> _batchKey = null!;
    private ConfigEntry<KeyCode> _markKey = null!;
    private ConfigEntry<KeyCode> _recordKey = null!;
    private Harmony? _harmony;
    private string _status = "";
    private string _flash = "";
    private float _flashUntil;
    private GUIStyle? _style;

    internal static string OutputDir { get; } = Path.Combine(Paths.BepInExRootPath, "FsmDump");

    private string Keys => $"{_recordKey.Value} start/stop | {_markKey.Value} mark | {_dumpKey.Value} dump";

    private void Awake()
    {
        _dumpKey = Config.Bind("Keys", "Dump", KeyCode.F9, "Write every loaded FSM to a file");
        const string batchHelp = "Load and dump every scene (use a throwaway save)";
        _batchKey = Config.Bind("Keys", "BatchDump", KeyCode.F8, batchHelp);
        _markKey = Config.Bind("Keys", "Mark", KeyCode.F10, "Write a numbered marker into the trace");
        _recordKey = Config.Bind("Keys", "Record", KeyCode.F11, "Start or stop recording the trace");
        Directory.CreateDirectory(OutputDir);
        _harmony = new Harmony("articha.fsmdump");
        _harmony.PatchAll(typeof(FsmDumpPlugin).Assembly);
        _status = $"FsmDump: stopped | {Keys}";
    }

    private void Update()
    {
        if (Trace.LastBossTitle is { } title)
        {
            Trace.LastBossTitle = null;
            Flash($"BOSS TITLE: {title}");
        }
        if (Input.GetKeyDown(_recordKey.Value))
        {
            if (Trace.IsRecording)
            {
                StopRecording();
            }
            else
            {
                StartRecording();
            }
        }
        if (Input.GetKeyDown(_markKey.Value))
        {
            string idle = $"not recording: press {_recordKey.Value}";
            Flash(Trace.IsRecording ? $"MARK {Trace.Mark()} written" : idle);
        }
        if (Input.GetKeyDown(_batchKey.Value) && !BatchDump.IsRunning)
        {
            StartBatch();
        }
        if (Input.GetKeyDown(_dumpKey.Value))
        {
            string path = Path.Combine(OutputDir, $"dump_{SceneTag()}.txt");
            int count = FsmWriter.DumpAll(path);
            string objects = Path.Combine(OutputDir, $"objects_{SceneTag()}.txt");
            int objectCount = ObjectWriter.DumpScene(objects);
            Flash($"DUMPED {count} FSMs, {objectCount} objects -> {Path.GetFileName(path)}");
        }
    }

    /// <summary>GOG only: the Steam copy may push achievements or stats while scenes load.</summary>
    private void StartBatch()
    {
        if (!File.Exists(Path.Combine(Paths.GameRootPath, "Galaxy64.dll")))
        {
            Flash("batch dump runs only in the GOG copy");
            return;
        }
        StartCoroutine(BatchDump.Run());
    }

    private void StartRecording()
    {
        string name = $"trace_{SceneTag()}.txt";
        Trace.Open(Path.Combine(OutputDir, name));
        _status = $"FsmDump: RECORDING {name}  |  {Keys}";
        Flash("recording started");
    }

    private void StopRecording()
    {
        Trace.Close();
        _status = $"FsmDump: stopped  |  {Keys}";
        Flash("recording stopped, file saved");
    }

    private void Flash(string text)
    {
        Logger.LogInfo(text);
        _flash = text;
        _flashUntil = Time.unscaledTime + 3f;
    }

    private void OnGUI()
    {
        _style ??= new GUIStyle(GUI.skin.box) { fontSize = 20, alignment = TextAnchor.MiddleLeft };
        GUI.color = Trace.IsRecording ? Color.red : Color.white;
        GUI.Box(new Rect(10, 10, 900, 34), _status, _style);
        GUI.color = Color.white;
        if (BatchDump.Progress != "")
        {
            GUI.Box(new Rect(10, 90, 900, 34), $"batch: {BatchDump.Progress}", _style);
        }
        if (Time.unscaledTime < _flashUntil)
        {
            GUI.Box(new Rect(10, 50, 900, 34), _flash, _style);
        }
    }

    private void OnDestroy()
    {
        Trace.Close();
        _harmony?.UnpatchSelf();
    }

    private static string SceneTag() => $"{SceneManager.GetActiveScene().name}_{DateTime.Now:HHmmss}";
}
