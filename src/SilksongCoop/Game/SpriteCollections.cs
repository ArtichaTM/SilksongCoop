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
    private static float _lastScan = -RescanCooldown;

    public static int HashOf(tk2dSpriteCollectionData collection) => Fnv.Hash(collection.name);

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
            return found;
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
            return found;
        }
        if (Reported.Add(~hash))
        {
            Coop.Current?.Log.LogWarning($"partner's sprite collection {hash:x8} is not loaded here");
        }
        return null;
    }

    public static void Clear() => Known.Clear();
}
