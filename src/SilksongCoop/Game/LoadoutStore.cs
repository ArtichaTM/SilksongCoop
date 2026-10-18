using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SilksongCoop.Rules;

namespace SilksongCoop.Game;

/// <summary>D20, D28: the friend's crest, tools and skills, one file per host on the friend's PC.</summary>
internal static class LoadoutStore
{
    private static string Folder => Path.Combine(Paths.ConfigPath, "SilksongCoop");

    public static string FileFor(string host, int port)
    {
        string safe = string.Concat($"{host}_{port}".Split(Path.GetInvalidFileNameChars()));
        return Path.Combine(Folder, $"loadout_{safe}.json");
    }

    public static void Save(string file, PlayerData data)
    {
        JsonSerializer json = Serializer();
        JObject loadout = [];
        foreach (string name in FieldRules.Loadout)
        {
            object? value = Field(name).GetValue(data);
            loadout[name] = value is null ? JValue.CreateNull() : JToken.FromObject(value, json);
        }
        Directory.CreateDirectory(Folder);
        File.WriteAllText(file, loadout.ToString(Formatting.Indented));
    }

    /// <summary>
    /// Puts the stored loadout into a save before it loads; false when there is none yet. A loadout using
    /// anything the host's save has locked is deleted: the friend takes the host's, as on a first join.
    /// </summary>
    public static bool Restore(string file, PlayerData data, out string? mismatch)
    {
        mismatch = null;
        if (!File.Exists(file))
        {
            return false;
        }
        JsonSerializer json = Serializer();
        JObject loadout = JObject.Parse(File.ReadAllText(file));
        T? Read<T>(string name) where T : class => loadout[name]?.ToObject<T>(json);
        string? current = Read<string>(nameof(PlayerData.CurrentCrestID));
        string? previous = Read<string>(nameof(PlayerData.PreviousCrestID));
        ToolCrestsData equips = Read<ToolCrestsData>(nameof(PlayerData.ToolEquips)) ?? new();
        FloatingCrestSlotsData extra = Read<FloatingCrestSlotsData>(nameof(PlayerData.ExtraToolEquips))
            ?? new();
        mismatch = Mismatch(data, [current, previous], equips, extra);
        if (mismatch is not null)
        {
            File.Delete(file);
            return false;
        }
        data.CurrentCrestID = string.IsNullOrEmpty(current) ? data.CurrentCrestID : current;
        data.PreviousCrestID = previous;
        EquipCrests(data.ToolEquips, equips);
        foreach (string slot in extra.GetValidNames())
        {
            ToolCrestsData.SlotData hosts = data.ExtraToolEquips.GetData(slot);
            hosts.EquippedTool = extra.GetData(slot).EquippedTool;
            data.ExtraToolEquips.SetData(slot, hosts);
        }
        return true;
    }

    /// <summary>What the stored loadout has unlocked that the host's save doesn't, or null.</summary>
    private static string? Mismatch(
        PlayerData host,
        string?[] crests,
        ToolCrestsData equips,
        FloatingCrestSlotsData extra
    )
    {
        foreach (string? crest in crests)
        {
            if (!string.IsNullOrEmpty(crest) && !host.ToolEquips.GetData(crest).IsUnlocked)
            {
                return $"crest {crest} is locked";
            }
        }
        foreach (string crest in equips.GetValidNames())
        {
            ToolCrestsData.Data mine = equips.GetData(crest);
            ToolCrestsData.Data hosts = host.ToolEquips.GetData(crest);
            if (mine.IsUnlocked && !hosts.IsUnlocked)
            {
                return $"crest {crest} is locked";
            }
            List<ToolCrestsData.SlotData> slots = mine.Slots ?? [];
            for (int index = 0; index < slots.Count; index++)
            {
                bool hostUnlocked = index < (hosts.Slots?.Count ?? 0) && hosts.Slots![index].IsUnlocked;
                if (slots[index].IsUnlocked && !hostUnlocked)
                {
                    return $"slot {index} of crest {crest} is locked";
                }
                if (LockedTool(host, slots[index]) is { } tool)
                {
                    return $"tool {tool} is locked";
                }
            }
        }
        foreach (string slot in extra.GetValidNames())
        {
            ToolCrestsData.SlotData mine = extra.GetData(slot);
            if (mine.IsUnlocked && !host.ExtraToolEquips.GetData(slot).IsUnlocked)
            {
                return $"extra slot {slot} is locked";
            }
            if (LockedTool(host, mine) is { } tool)
            {
                return $"tool {tool} is locked";
            }
        }
        return null;
    }

    private static string? LockedTool(PlayerData host, ToolCrestsData.SlotData slot)
    {
        string tool = slot.EquippedTool;
        return string.IsNullOrEmpty(tool) || host.Tools.GetData(tool).IsUnlocked ? null : tool;
    }

    /// <summary>Only the equipped tools move over; unlock flags stay the host's, it may have more.</summary>
    private static void EquipCrests(ToolCrestsData host, ToolCrestsData equips)
    {
        foreach (string crest in equips.GetValidNames())
        {
            List<ToolCrestsData.SlotData>? slots = equips.GetData(crest).Slots;
            if (slots is null)
            {
                continue;
            }
            ToolCrestsData.Data hosts = host.GetData(crest);
            hosts.Slots ??= new List<ToolCrestsData.SlotData>(slots.Count);
            for (int index = 0; index < slots.Count; index++)
            {
                string tool = slots[index].EquippedTool;
                if (index < hosts.Slots.Count)
                {
                    ToolCrestsData.SlotData slot = hosts.Slots[index];
                    slot.EquippedTool = tool;
                    hosts.Slots[index] = slot;
                }
                else
                {
                    hosts.Slots.Add(new ToolCrestsData.SlotData { EquippedTool = tool });
                }
            }
            host.SetData(crest, hosts);
        }
    }

    private static FieldInfo Field(string name)
    {
        FieldInfo? field = typeof(PlayerData).GetField(name, BindingFlags.Public | BindingFlags.Instance);
        return field ?? throw new MissingFieldException(nameof(PlayerData), name);
    }

    private static JsonSerializer Serializer()
    {
        SaveDataUtility.CreateJsonObjects();
        return SaveDataUtility._serializer;
    }
}
