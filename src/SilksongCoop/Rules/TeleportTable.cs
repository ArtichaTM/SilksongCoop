using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace SilksongCoop.Rules;

/// <summary>One object of a verdict row (docs/sync-audit/teleports.tsv); X and Y null without one.</summary>
internal sealed record TeleportTarget(
    string Id,
    string Row,
    string Element,
    string Scene,
    string Load,
    string Door,
    string Path,
    float? X,
    float? Y);

/// <summary>A verdict row's objects in one scene, and the scene that loads it.</summary>
internal sealed record TeleportRow(
    string Id,
    string Scene,
    string Label,
    IReadOnlyList<TeleportTarget> Targets)
{
    public string Load => Targets[0].Load;

    public string Door => Targets[0].Door;

    /// <summary>The objects to step through; empty for a row whose element has no object.</summary>
    public IReadOnlyList<TeleportTarget> Objects { get; } = Targets.Where(t => t.Path.Length > 0).ToList();
}

/// <summary>Verdict rows grouped by verdict file, then scene, in the order the file lists them.</summary>
internal static class TeleportTable
{
    public static List<TeleportTarget> Parse(string tsv)
    {
        List<TeleportTarget> targets = [];
        foreach (string line in tsv.Split('\n').Skip(1))
        {
            string[] cells = line.TrimEnd('\r').Split('\t');
            if (cells.Length < 9)
            {
                continue;
            }
            float? x = Number(cells[7]);
            float? y = Number(cells[8]);
            bool placed = x is not null && y is not null;
            targets.Add(new(cells[0], cells[1], cells[2], cells[3], cells[4], cells[5], cells[6],
                placed ? x : null, placed ? y : null));
        }
        return targets;
    }

    public static List<TeleportRow> Rows(IEnumerable<TeleportTarget> targets)
    {
        return targets
            .GroupBy(t => (t.Id, t.Scene))
            .Select(g => new TeleportRow(g.Key.Id, g.Key.Scene, Label(g.Key.Id, g.ToList()), g.ToList()))
            .ToList();
    }

    /// <summary>The verdict file of an id: `02` for `02.01`.</summary>
    public static string File(string id) => id.Split('.')[0];

    /// <summary>A case-insensitive part of the id, audit rows, elements or scene.</summary>
    public static bool Matches(TeleportRow row, string query)
    {
        query = query.Trim();
        if (query.Length == 0)
        {
            return true;
        }
        string all = $"{row.Label} {row.Scene}";
        return all.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    /// <summary>The object after <paramref name="current"/>, wrapping; pass -1 for the first press.</summary>
    public static int Next(TeleportRow row, int current) =>
        row.Objects.Count == 0 ? -1 : (current + 1) % row.Objects.Count;

    private static string Label(string id, List<TeleportTarget> targets)
    {
        IEnumerable<string> rows = targets.Select(t => t.Row).Distinct();
        IEnumerable<string> elements = targets.Select(t => t.Element).Distinct();
        return $"{id}  {string.Join(", ", rows)}  {string.Join("; ", elements)}";
    }

    private static float? Number(string cell)
    {
        const NumberStyles Style = NumberStyles.Float;
        return float.TryParse(cell, Style, CultureInfo.InvariantCulture, out float value) ? value : null;
    }
}
