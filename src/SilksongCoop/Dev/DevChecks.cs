using System;
using System.Collections.Generic;
using System.Linq;
using HutongGames.PlayMaker;
using SilksongCoop.Game;
using UnityEngine;

namespace SilksongCoop.Dev;

/// <summary>
/// What dev mode looks for: control that never comes back, the games stuck in different scenes,
/// exceptions. Each finding once per scene visit.
/// </summary>
internal sealed class DevChecks : IDisposable
{
    private const float ControlTimeout = 10f;
    private const float SceneTimeout = 15f;
    private const int ExceptionLimit = 20;

    private readonly Coop _coop;
    private readonly DevReport _report;
    private readonly HashSet<string> _exceptions = [];
    private float? _lostSince;
    private bool _lostReported;
    private float? _apartSince;
    private bool _apartReported;
    private string? _scene;

    public DevChecks(Coop coop, DevReport report)
    {
        _coop = coop;
        _report = report;
        Application.logMessageReceived += OnLog;
    }

    private void NewScene()
    {
        _exceptions.Clear();
        _lostSince = null;
        _lostReported = false;
        _apartSince = null;
        _apartReported = false;
    }

    public void Update()
    {
        string? scene = GameRefs.Manager?.sceneName;
        if (scene != _scene)
        {
            _scene = scene;
            _report.Line("entered");
            NewScene();
        }
        CheckControl();
        CheckScenes();
    }

    public void Dispose() => Application.logMessageReceived -= OnLog;

    private void CheckControl()
    {
        HeroController? hero = GameRefs.Hero;
        GameManager? gm = GameRefs.Manager;
        if (hero is null || gm is null || !GameRefs.HeroReady || gm.isPaused || hero.cState.dead)
        {
            return;
        }
        float now = Time.unscaledTime;
        if (hero.acceptingInput && !hero.controlReqlinquished)
        {
            if (_lostReported)
            {
                _report.Line($"control back after {now - _lostSince:0} s");
            }
            _lostSince = null;
            _lostReported = false;
            return;
        }
        _lostSince ??= now;
        if (!_lostReported && now - _lostSince > ControlTimeout)
        {
            _lostReported = true;
            string flags = Patches.HeroControlLogPatches.Describe(hero);
            string where = $"{ControlTimeout:0} s in {gm.sceneName}";
            _report.Line($"control not back for {where}; {flags}; {CutsceneStates()}");
            _coop.Log.LogWarning($"control not back for {ControlTimeout:0} s: {flags}");
        }
    }

    private void CheckScenes()
    {
        string? here = _coop.Rooms.Current;
        if (!_coop.InCoop || here is null)
        {
            _apartSince = null;
            return;
        }
        string partner = _coop.Partner.Scene ?? _coop.Partner.Heading ?? "nowhere";
        if (partner == here)
        {
            _apartSince = null;
            _apartReported = false;
            return;
        }
        float now = Time.unscaledTime;
        _apartSince ??= now;
        if (!_apartReported && now - _apartSince > SceneTimeout)
        {
            _apartReported = true;
            _report.Line($"games apart for {SceneTimeout:0} s: here {here}, partner {partner}");
        }
    }

    private void OnLog(string condition, string stackTrace, LogType type)
    {
        if (type != LogType.Exception || _exceptions.Count >= ExceptionLimit || !_exceptions.Add(condition))
        {
            return;
        }
        string where = stackTrace.Split('\n').FirstOrDefault()?.Trim() ?? "";
        _report.Line($"exception: {condition} at {where}");
    }

    /// <summary>Which hero cutscenes are running, to name the one that kept control.</summary>
    private static string CutsceneStates()
    {
        PlayMakerFSM[] active = UnityEngine.Object.FindObjectsByType<PlayMakerFSM>(FindObjectsSortMode.None);
        IEnumerable<string> running = active
            .Where(fsm => HeroCutscenes.Drives(fsm.Fsm))
            .Select(fsm => $"{fsm.gameObject.name} [{fsm.FsmName}] at {fsm.ActiveStateName}");
        string list = string.Join("; ", running);
        return list.Length > 0 ? $"hero cutscenes: {list}" : "no hero cutscene FSM running";
    }
}
