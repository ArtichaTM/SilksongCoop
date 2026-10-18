using System.Collections.Generic;
using SilksongCoop.Rules;
using UnityEngine;

namespace SilksongCoop.Game;

/// <summary>Plain Unity sprites (thrown tools) by name hash, like <see cref="SpriteCollections"/>.</summary>
internal static class FlatSprites
{
    /// <summary>The collection hash a frame carries when it is a Unity sprite, not a tk2d one.</summary>
    public const int Marker = 0;

    private const float RescanCooldown = 1f;
    private static readonly Dictionary<int, Sprite> Known = [];
    private static readonly HashSet<int> Reported = [];
    private static float _lastScan = -RescanCooldown;

    public static int Sent(Sprite sprite)
    {
        int hash = Fnv.Hash(sprite.name);
        if (Reported.Add(hash))
        {
            Coop.Current?.Log.LogInfo($"sending sprite {sprite.name} ({hash:x8})");
        }
        return hash;
    }

    public static Sprite? Find(int hash)
    {
        if (Known.TryGetValue(hash, out Sprite? found) && found)
        {
            return found;
        }
        if (Time.unscaledTime - _lastScan < RescanCooldown)
        {
            return null;
        }
        _lastScan = Time.unscaledTime;
        foreach (Sprite sprite in Resources.FindObjectsOfTypeAll<Sprite>())
        {
            Known[Fnv.Hash(sprite.name)] = sprite;
        }
        if (Known.TryGetValue(hash, out found) && found)
        {
            return found;
        }
        if (Reported.Add(~hash))
        {
            Coop.Current?.Log.LogWarning($"partner's sprite {hash:x8} is not loaded here");
        }
        return null;
    }
}
