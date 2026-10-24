using BepInEx;
using HarmonyLib;

namespace SilksongCoop;

[BepInPlugin(Guid, Name, Version)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string Guid = "articha.silksongcoop";
    public const string Name = "SilksongCoop";
    public const string Version = "0.1.0";

    private Harmony? _harmony;
    private Coop? _coop;

    /// <summary>Set when Awake ends; the loader reads it by name, as Unity swallows Awake's errors.</summary>
    public bool IsUp { get; private set; }

    private void Awake()
    {
        _coop = new Coop(this, new CoopConfig(Config), Logger);
        _harmony = new Harmony(Guid);
        _harmony.PatchAll(typeof(Plugin).Assembly);
        Logger.LogInfo($"{Name} {Version} loaded, protocol {Handshake.ProtocolVersion}");
        IsUp = true;
    }

    private void Update() => _coop?.Update();

    private void LateUpdate() => _coop?.LateUpdate();

    private void OnGUI() => _coop?.OnGUI();

    /// <summary>C2: hot reload must leave nothing behind.</summary>
    private void OnDestroy()
    {
        _harmony?.UnpatchSelf();
        _coop?.Dispose();
        _coop = null;
    }
}
