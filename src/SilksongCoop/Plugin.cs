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

    private void Awake()
    {
        _harmony = new Harmony(Guid);
        _harmony.PatchAll(typeof(Plugin).Assembly);
        Logger.LogInfo($"{Name} {Version} loaded, protocol {Handshake.ProtocolVersion}");
    }

    private void OnDestroy() => _harmony?.UnpatchSelf();
}
