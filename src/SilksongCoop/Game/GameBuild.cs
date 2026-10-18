using System.Reflection;

namespace SilksongCoop.Game;

/// <summary>D49: game version plus the Assembly-CSharp module id, which changes every build.</summary>
internal static class GameBuild
{
    public static string Fingerprint { get; } = Compute();

    private static string Compute()
    {
        // A const would be baked into this plugin at compile time; read the running game's value instead.
        FieldInfo? field = typeof(Constants).GetField(nameof(Constants.GAME_VERSION));
        object? version = field?.GetRawConstantValue();
        string module = typeof(GameManager).Assembly.ManifestModule.ModuleVersionId.ToString("N");
        return $"{version} ({module.Substring(0, 8)})";
    }
}
