using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
    private const string Repo = "ArtichaTM/SilksongCoop";
    private const string Nightly = "https://github.com/" + Repo + "/releases/download/nightly/";
    private const string DllUrl = Nightly + DllName;
    private const string SymbolsUrl = Nightly + "SilksongCoop.pdb";

    /// <summary>Lists each asset's SHA-256, which GitHub computes on upload.</summary>
    private const string ReleaseApiUrl = "https://api.github.com/repos/" + Repo + "/releases/tags/nightly";
    private const string DigestPrefix = "sha256:";
    private const int TimeoutSeconds = 10;
    private const float OfflineNoticeSeconds = 10f;

    private ConfigEntry<bool> _autoUpdate = null!;
    private ConfigEntry<bool> _symbols = null!;
    private string? _status;
    private float _statusUntil = float.PositiveInfinity;
    private bool _failed;
    private GUIStyle? _rightAligned;

    private string PayloadPath => Path.Combine(Path.GetDirectoryName(Info.Location), "SilksongCoop.payload");

    /// <summary>The host's ETag of the payload on disk; sent back so an unchanged file costs a 304.</summary>
    private string EtagPath => PayloadPath + ".etag";

    /// <summary>The payload's pdb; deleted whenever a new payload lands, so it never mismatches.</summary>
    private string SymbolsPath => PayloadPath + ".pdb";

    private static string DevInstall => Path.Combine(Paths.BepInExRootPath, "scripts", Folder, DllName);

    private void Awake()
    {
        _autoUpdate = Config.Bind("Update", "Enabled", true, "Download a newer SilksongCoop on game start");
        _symbols = Config.Bind("Update", "Symbols", false, "Also download the .pdb: errors get line numbers");
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
            if (_symbols.Value && File.Exists(PayloadPath) && !File.Exists(SymbolsPath))
            {
                yield return DownloadSymbols();
            }
        }
        Run();
    }

    /// <summary>Best effort: without the pdb SilksongCoop still runs, its errors just lack lines.</summary>
    private IEnumerator DownloadSymbols()
    {
        using UnityWebRequest request = UnityWebRequest.Get(SymbolsUrl);
        request.timeout = TimeoutSeconds;
        yield return request.SendWebRequest();
        if (request.result != UnityWebRequest.Result.Success)
        {
            Logger.LogWarning($"SilksongCoop pdb not downloaded: {request.error}");
            yield break;
        }
        File.WriteAllBytes(SymbolsPath, request.downloadHandler.data);
    }

    private void OnGUI()
    {
        if (_status is not null && Time.realtimeSinceStartup < _statusUntil)
        {
            _rightAligned ??= new GUIStyle(GUI.skin.label) { alignment = TextAnchor.UpperRight };
            GUI.Label(new Rect(Screen.width - 810, 10, 800, 60), _status, _rightAligned);
        }
    }

    private IEnumerator Download()
    {
        using UnityWebRequest request = UnityWebRequest.Get(DllUrl);
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
        _status = "SilksongCoop: verifying the download";
        string? digest = null;
        yield return FetchDigest(found => digest = found);
        if (digest is null)
        {
            Refuse("GitHub did not report the build's checksum");
            yield break;
        }
        string hash = Sha256(payload);
        if (!string.Equals(hash, digest, StringComparison.OrdinalIgnoreCase))
        {
            Logger.LogWarning($"downloaded sha256 {hash}, release lists {digest}");
            Refuse("the checksum does not match, the release may be mid-upload");
            yield break;
        }
        string etag = request.GetResponseHeader("ETag");
        string? current = File.Exists(PayloadPath) ? Sha256(File.ReadAllBytes(PayloadPath)) : null;
        if (string.Equals(current, hash, StringComparison.OrdinalIgnoreCase))
        {
            Logger.LogInfo("SilksongCoop is up to date, the release was re-uploaded");
            SaveEtag(etag);
            yield break;
        }
        Save(payload, etag);
        string built = request.GetResponseHeader("Last-Modified");
        Logger.LogMessage($"SilksongCoop updated to the build of {built}");
        _status = "SilksongCoop: starting";
    }

    /// <summary>Old payload and etag stay, so the next start downloads and checks again.</summary>
    private void Refuse(string reason) =>
        Fail($"SilksongCoop update refused: {reason}; retrying next start", OfflineNoticeSeconds);

    /// <summary>Passes the hex SHA-256 the release lists for the DLL, or null on any failure.</summary>
    private IEnumerator FetchDigest(Action<string?> found)
    {
        using UnityWebRequest request = UnityWebRequest.Get(ReleaseApiUrl);
        request.timeout = TimeoutSeconds;
        request.SetRequestHeader("User-Agent", $"{Name}/{Version}");
        request.SetRequestHeader("Accept", "application/vnd.github+json");
        yield return request.SendWebRequest();
        if (request.result != UnityWebRequest.Result.Success)
        {
            Logger.LogWarning($"release info not fetched: {request.error}");
            found(null);
            yield break;
        }
        JToken? release = null;
        try
        {
            release = JToken.Parse(request.downloadHandler.text);
        }
        catch (JsonException exception)
        {
            Logger.LogWarning($"release info unreadable: {exception.Message}");
        }
        string? digest = (release?["assets"] as JArray)?.OfType<JObject>()
            .FirstOrDefault(asset => (string?)asset["name"] == DllName)?["digest"]?.Value<string>();
        if (digest is null || !digest.StartsWith(DigestPrefix, StringComparison.Ordinal))
        {
            Logger.LogWarning($"release info has no {DigestPrefix} digest for {DllName}");
            found(null);
            yield break;
        }
        found(digest.Substring(DigestPrefix.Length));
    }

    private static string Sha256(byte[] data)
    {
        using SHA256 sha = SHA256.Create();
        return BitConverter.ToString(sha.ComputeHash(data)).Replace("-", "");
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
        File.Delete(SymbolsPath);
        SaveEtag(etag);
    }

    private void SaveEtag(string? etag)
    {
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
            string why = _failed ? "\n" + _status : "";
            Fail($"SilksongCoop is not downloaded yet; expected at {PayloadPath}{why}");
            return;
        }
        try
        {
            bool symbols = _symbols.Value && File.Exists(SymbolsPath);
            byte[]? pdb = symbols ? File.ReadAllBytes(SymbolsPath) : null;
            Assembly assembly = Assembly.Load(File.ReadAllBytes(PayloadPath), pdb);
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
