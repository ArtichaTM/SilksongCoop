namespace SilksongCoop;

/// <summary>Join admission (D32, D34, D49): same mod version, same game build, shared password.</summary>
internal static class Handshake
{
    public const string ProtocolVersion = Plugin.Version;

    /// <summary>Null when the client may join, otherwise the reason shown to the client.</summary>
    public static string? Reject(Credentials host, Credentials client)
    {
        if (host.Version != client.Version)
        {
            return $"mod version mismatch: host {host.Version}, you {client.Version}";
        }
        if (host.Build != client.Build)
        {
            return $"game build mismatch: host {host.Build}, you {client.Build}";
        }
        return host.Password == client.Password ? null : "wrong password";
    }
}

internal readonly record struct Credentials(string Version, string Build, string Password);
