using System.Reflection;

namespace SilksongCoop;

/// <summary>Join admission (D32, D34, D49): same mod build, same game build, shared password.</summary>
internal static class Handshake
{
    /// <summary>The mod version plus its build's commit, so two nightlies never pass as one.</summary>
    public static readonly string ProtocolVersion = WithCommit(Plugin.Version, Stamp());

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

    /// <summary>The SDK stamps <c>version+sha</c>; a build outside git has no sha.</summary>
    internal static string WithCommit(string version, string? informational)
    {
        int plus = informational?.IndexOf('+') ?? -1;
        if (plus < 0)
        {
            return version;
        }
        string sha = informational!.Substring(plus + 1);
        return $"{version}+{(sha.Length > 7 ? sha.Substring(0, 7) : sha)}";
    }

    private static string? Stamp()
    {
        Assembly assembly = typeof(Handshake).Assembly;
        return assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
    }
}

internal readonly record struct Credentials(string Version, string Build, string Password);
