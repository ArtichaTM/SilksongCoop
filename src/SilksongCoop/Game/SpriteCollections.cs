using System.Collections.Generic;
using SilksongCoop.Rules;
using UnityEngine;

namespace SilksongCoop.Game;

/// <summary>Finds a loaded tk2d sprite collection by the name hash the partner sent.</summary>
internal static class SpriteCollections
{
    private const float RescanCooldown = 1f;
    private static readonly Dictionary<int, tk2dSpriteCollectionData> Known = [];
    private static readonly HashSet<int> Reported = [];
    private static readonly HashSet<int> Checks = [];
    private static readonly HashSet<(int, int)> Drawn = [];
    private static float _lastScan = -RescanCooldown;

    private static readonly Dictionary<tk2dSpriteCollectionData, int> Hashes = [];

    /// <summary>Different sheets share names (two "Tools Cln"), so their contents count too.</summary>
    public static int HashOf(tk2dSpriteCollectionData collection)
    {
        if (!Hashes.TryGetValue(collection, out int hash))
        {
            tk2dSpriteDefinition[]? definitions = collection.spriteDefinitions;
            int count = definitions?.Length ?? 0;
            string first = count > 0 ? definitions![0].name : "";
            hash = Fnv.Hash($"{collection.name}/{count}/{first}");
            Hashes[collection] = hash;
        }
        return hash;
    }

    /// <summary>Sender side: logs each collection once, so a receiver's missing hash can be named.</summary>
    public static int Sent(tk2dSpriteCollectionData collection)
    {
        int hash = HashOf(collection);
        if (Reported.Add(hash))
        {
            Coop.Current?.Log.LogInfo($"sending sprite collection {collection.name} ({hash:x8})");
        }
        return hash;
    }

    public static tk2dSpriteCollectionData? Find(int hash)
    {
        if (Known.TryGetValue(hash, out tk2dSpriteCollectionData? found) && found)
        {
            return Checked(hash, found);
        }
        if (Time.unscaledTime - _lastScan < RescanCooldown)
        {
            return null;
        }
        _lastScan = Time.unscaledTime;
        foreach (tk2dSpriteCollectionData data in Resources.FindObjectsOfTypeAll<tk2dSpriteCollectionData>())
        {
            Known[HashOf(data)] = data;
        }
        if (Known.TryGetValue(hash, out found) && found)
        {
            return Checked(hash, found);
        }
        if (Reported.Add(~hash))
        {
            Coop.Current?.Log.LogWarning($"partner's sprite collection {hash:x8} is not loaded here");
        }
        return null;
    }

    public static void Clear()
    {
        Known.Clear();
        Hashes.Clear();
    }

    /// <summary>Receiver diagnostic: each partner tool sprite once, as actually drawn here.</summary>
    public static void ReportDrawn(SpriteFrame frame, tk2dSprite sprite)
    {
        if (!Drawn.Add((frame.Collection, frame.SpriteId)) || sprite.CurrentSprite is not { } definition)
        {
            return;
        }
        MeshRenderer renderer = sprite.GetComponent<MeshRenderer>();
        Material material = renderer.sharedMaterial;
        string texture = material && material.mainTexture ? "textured" : "untextured";
        string sheet = $"{sprite.Collection.name}/{definition.name}";
        string where = $"{sprite.transform.position} scale {sprite.transform.lossyScale}";
        Coop.Current?.Log.LogInfo($"drawing partner tool {sheet} with {(material ? material.name : "none")} "
            + $"({texture}), a={frame.Color.a}, active={sprite.gameObject.activeInHierarchy}, at {where}");
    }

    /// <summary>Receiver side: a sheet whose texture never loaded draws invisible frames.</summary>
    private static tk2dSpriteCollectionData Checked(int hash, tk2dSpriteCollectionData found)
    {
        if (Checks.Add(hash))
        {
            Material[]? materials = found.inst.materials;
            bool textured = materials is { Length: > 0 } && materials[0] && materials[0].mainTexture;
            if (!textured)
            {
                Coop.Current?.Log.LogWarning($"partner's sprite sheet {found.name} has no texture here");
            }
        }
        return found;
    }
}
