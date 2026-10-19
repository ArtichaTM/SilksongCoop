using System;
using System.IO;
using SilksongCoop.Game;

namespace SilksongCoop.Dev;

/// <summary>One report per dev session in each game: scenes visited and what went wrong there.</summary>
internal sealed class DevReport : IDisposable
{
    private readonly StreamWriter _writer;

    public DevReport()
    {
        Path = DevFiles.NewPath("report", "txt");
        _writer = new StreamWriter(Path) { AutoFlush = true };
    }

    public string Path { get; }

    public void Line(string text)
    {
        string scene = GameRefs.Manager?.sceneName ?? "-";
        _writer.WriteLine($"{DateTime.Now:HH:mm:ss} [{scene}] {text}");
    }

    public void Dispose() => _writer.Dispose();
}
