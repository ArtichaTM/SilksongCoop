using System;
using System.IO;
using BepInEx;

namespace SilksongCoop.Dev;

/// <summary>Where dev reports go.</summary>
internal static class DevFiles
{
    public static string Dir { get; } = Path.Combine(Paths.BepInExRootPath, "CoopDev");

    public static string NewPath(string kind, string extension)
    {
        Directory.CreateDirectory(Dir);
        return Path.Combine(Dir, $"{kind}_{DateTime.Now:yyyyMMdd_HHmmss}.{extension}");
    }
}
