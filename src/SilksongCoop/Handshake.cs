namespace SilksongCoop;

/// <summary>Connection admission (D32, D34): same mod version and the shared password.</summary>
internal static class Handshake
{
    public const string ProtocolVersion = Plugin.Version;

    /// <summary>Null when the client may join, otherwise the reason shown to the client.</summary>
    public static string? Reject(
        string hostVersion,
        string clientVersion,
        string hostPassword,
        string clientPassword
    )
    {
        if (hostVersion != clientVersion)
        {
            return $"version mismatch: host {hostVersion}, you {clientVersion}";
        }
        return hostPassword == clientPassword ? null : "wrong password";
    }
}
