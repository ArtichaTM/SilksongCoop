using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using UnityEngine;
using UnityEngine.Networking;

namespace SilksongCoop.Loader;

/// <summary>
/// Downloads SilksongCoop when the nightly build is newer, then starts it from memory, as ScriptEngine does.
/// The copy is stored as <c>.payload</c>, which the Chainloader never picks up on its own.
/// </summary>
[BepInPlugin(Guid, Name, Version)]
public sealed class Loader : BaseUnityPlugin
{
    public const string Guid = "articha.silksongcoop.loader";
    public const string Name = "SilksongCoop.Loader";
    public const string Version = "0.1.0";

    private const string Folder = "SilksongCoop";
    private const string DllName = "SilksongCoop.dll";
    private const string Releases = "https://github.com/ArtichaTM/SilksongCoop/releases/download";
    private const string DefaultUrl = Releases + "/nightly/" + DllName;
    private const int TimeoutSeconds = 10;
    private const float OfflineNoticeSeconds = 10f;

    private ConfigEntry<bool> _autoUpdate = null!;
    private ConfigEntry<string> _url = null!;
    private string? _status;
    private float _statusUntil = float.PositiveInfinity;
    private bool _failed;
    private GUIStyle? _rightAligned;

    private string PayloadPath => Path.Combine(Path.GetDirectoryName(Info.Location), "SilksongCoop.payload");

    /// <summary>The host's ETag of the payload on disk; sent back so an unchanged file costs a 304.</summary>
    private string EtagPath => PayloadPath + ".etag";

    private static string DevInstall => Path.Combine(Paths.BepInExRootPath, "scripts", Folder, DllName);

    private void Awake()
    {
        _autoUpdate = Config.Bind("Update", "Enabled", true, "Download a newer SilksongCoop on game start");
        _url = Config.Bind("Update", "Url", DefaultUrl, "Where the newest SilksongCoop.dll is served");
    }

    private IEnumerator Start()
    {
        if (File.Exists(DevInstall))
        {
            Logger.LogInfo($"dev install found at {DevInstall}, leaving SilksongCoop to ScriptEngine");
            yield break;
        }
        if (_autoUpdate.Value)
        {
            yield return Download();
        }
        Run();
    }

    private void OnGUI()
    {
        if (_status is not null && Time.realtimeSinceStartup < _statusUntil)
        {
            _rightAligned ??= new GUIStyle(GUI.skin.label) { alignment = TextAnchor.UpperRight };
            GUI.Label(new Rect(Screen.width - 810, 10, 800, 30), _status, _rightAligned);
        }
    }

    private IEnumerator Download()
    {
        using UnityWebRequest request = UnityWebRequest.Get(_url.Value);
        request.timeout = TimeoutSeconds;
        if (File.Exists(PayloadPath) && File.Exists(EtagPath))
        {
            request.SetRequestHeader("If-None-Match", File.ReadAllText(EtagPath));
        }
        UnityWebRequestAsyncOperation sending = request.SendWebRequest();
        while (!sending.isDone)
        {
            _status = Progress(request);
            yield return null;
        }

        if (request.responseCode == 304)
        {
            Logger.LogInfo("SilksongCoop is up to date");
            yield break;
        }
        if (request.result != UnityWebRequest.Result.Success)
        {
            bool offline = request.result == UnityWebRequest.Result.ConnectionError;
            Fail($"SilksongCoop update check failed: {request.error}", offline ? OfflineNoticeSeconds : null);
            yield break;
        }
        byte[] payload = request.downloadHandler.data;
        string? problem = Problem(payload, request.GetResponseHeader("Content-Length"));
        if (problem is not null)
        {
            Fail($"SilksongCoop update rejected: {problem}");
            yield break;
        }
        Save(payload, request.GetResponseHeader("ETag"));
        string built = request.GetResponseHeader("Last-Modified");
        Logger.LogMessage($"SilksongCoop updated to the build of {built}");
        _status = "SilksongCoop: starting";
    }

    private static string Progress(UnityWebRequest request)
    {
        long received = (long)request.downloadedBytes / 1024;
        return long.TryParse(request.GetResponseHeader("Content-Length"), out long total)
            ? $"SilksongCoop: downloading {received} / {total / 1024} KB"
            : "SilksongCoop: checking for update";
    }

    /// <summary>Null when <paramref name="payload"/> looks like a whole DLL, otherwise why not.</summary>
    private static string? Problem(byte[] payload, string? contentLength)
    {
        if (int.TryParse(contentLength, out int expected) && expected != payload.Length)
        {
            return $"got {payload.Length} of {expected} bytes";
        }
        return payload.Length < 2 || payload[0] != 'M' || payload[1] != 'Z' ? "not a DLL" : null;
    }

    /// <summary>Writes beside the old copy first, so a failed write leaves the old copy usable.</summary>
    private void Save(byte[] payload, string? etag)
    {
        string fresh = PayloadPath + ".new";
        File.WriteAllBytes(fresh, payload);
        File.Delete(PayloadPath);
        File.Move(fresh, PayloadPath);
        if (etag is null)
        {
            File.Delete(EtagPath);
        }
        else
        {
            File.WriteAllText(EtagPath, etag);
        }
    }

    private void Run()
    {
        if (!File.Exists(PayloadPath))
        {
            Fail($"SilksongCoop is not downloaded yet; expected at {PayloadPath}");
            return;
        }
        try
        {
            Assembly assembly = Assembly.Load(File.ReadAllBytes(PayloadPath));
            Type type = assembly.GetTypes().First(typeof(BaseUnityPlugin).IsAssignableFrom);
            string guid = MetadataHelper.GetMetadata(type).GUID;
            if (Chainloader.PluginInfos.TryGetValue(guid, out PluginInfo loaded))
            {
                Fail($"SilksongCoop is already loaded from {loaded.Location}; remove that copy");
                return;
            }
            BaseUnityPlugin plugin = (BaseUnityPlugin)gameObject.AddComponent(type);
            Chainloader.PluginInfos[guid] = plugin.Info;
            if (type.GetProperty("IsUp")?.GetValue(plugin) is false)
            {
                Fail("SilksongCoop failed to start, see LogOutput.log");
                return;
            }
            _status = _failed ? _status : null;
        }
        catch (Exception exception)
        {
            Fail($"SilksongCoop failed to start: {exception.Message}");
            Logger.LogError(exception);
        }
    }

    /// <summary>Shown <paramref name="seconds"/> or all session, even after SilksongCoop starts.</summary>
    private void Fail(string message, float? seconds = null)
    {
        Logger.LogError(message);
        _status = message;
        _statusUntil = Time.realtimeSinceStartup + (seconds ?? float.PositiveInfinity);
        _failed = true;
    }
}
