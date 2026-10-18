using System.Diagnostics;

namespace SilksongCoop.Net;

/// <summary>Seconds since load; any thread, unaffected by time scale or frame stalls.</summary>
internal static class Clock
{
    private static readonly Stopwatch Watch = Stopwatch.StartNew();

    public static double Now => Watch.Elapsed.TotalSeconds;
}
