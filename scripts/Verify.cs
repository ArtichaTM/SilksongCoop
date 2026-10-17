// Every check, one line per step: `dotnet run scripts/Verify.cs` from the repo root.
// A step's own output is shown only when it fails; the first failure stops the run.

using System.Diagnostics;
using System.Text;

const string Solution = "SilksongCoop.slnx";
const string Lenient = "-p:RunAnalyzers=false -p:TreatWarningsAsErrors=false";
const int MaxWidth = 110;
string[] measured = ["*.cs", "*.csproj", "*.props", "*.targets", "*.slnx", ":!docs/", ":!data/"];

bool passed = Step("restore", $"restore {Solution} --locked-mode")
    && Step("build", $"build {Solution} --no-restore -c Release {Lenient}")
    && Group("lint")
    && Step("- analyzers", $"build {Solution} --no-restore -c Release")
    && Step("- whitespace", $"format whitespace {Solution} --no-restore --verify-no-changes")
    && Check("- width", WideLines)
    && Step("test", $"test --project tests/SilksongCoop.Tests --no-restore -c Release {Lenient}");
Console.Write("verify... ");
Status(passed);
return passed ? 0 : 1;

static bool Group(string name)
{
    Console.WriteLine($"{name}:");
    return true;
}

static bool Step(string name, string arguments)
{
    return Check(name, () =>
    {
        (int code, string output) = Run("dotnet", arguments);
        return code == 0 ? null : output;
    });
}

/// <summary>Runs <paramref name="problems"/>; null means ok, anything else is printed under FAILED.</summary>
static bool Check(string name, Func<string?> problems)
{
    Console.Write($"{name}... ");
    string? found = problems();
    Status(found is null);
    Console.Write(found);
    return found is null;
}

/// <summary>Green ok or red FAILED; the runtime drops the colour when output is not a terminal.</summary>
static void Status(bool ok)
{
    Console.ForegroundColor = ok ? ConsoleColor.Green : ConsoleColor.Red;
    Console.Write(ok ? "ok" : "FAILED");
    Console.ResetColor();
    Console.WriteLine();
}

static (int Code, string Output) Run(string file, string arguments)
{
    ProcessStartInfo start = new(file, arguments)
    {
        RedirectStandardOutput = true,
        RedirectStandardError = true,
    };
    using Process process = Process.Start(start)!;
    Task<string> error = process.StandardError.ReadToEndAsync();
    string output = process.StandardOutput.ReadToEnd();
    process.WaitForExit();
    return (process.ExitCode, output + error.Result);
}

/// <summary>Tracked and untracked files, so new files fail here too.</summary>
string? WideLines()
{
    (int code, string files) = Run("git", $"ls-files -co --exclude-standard {string.Join(' ', measured)}");
    if (code != 0)
    {
        return files;
    }
    StringBuilder wide = new();
    const StringSplitOptions Clean = StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries;
    foreach (string path in files.Split('\n', Clean).Where(File.Exists))
    {
        string[] lines = File.ReadAllLines(path);
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].EnumerateRunes().Count() > MaxWidth)
            {
                wide.AppendLine($"{path}:{i + 1}: over {MaxWidth} columns");
            }
        }
    }
    return wide.Length == 0 ? null : wide.ToString();
}
