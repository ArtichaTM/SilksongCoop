using System;
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

    /// <summary>Puts the stored loadout into a save before it loads; false when there is none yet.</summary>
    public static bool Restore(string file, PlayerData data)
    {
        if (!File.Exists(file))
        {
            return false;
        }
        JsonSerializer json = Serializer();
        JObject loadout = JObject.Parse(File.ReadAllText(file));
        foreach (string name in FieldRules.Loadout)
        {
            if (loadout.TryGetValue(name, out JToken? token))
            {
                FieldInfo field = Field(name);
                field.SetValue(data, token.ToObject(field.FieldType, json));
            }
        }
        return true;
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
