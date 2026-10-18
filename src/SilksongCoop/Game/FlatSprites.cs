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
    private static readonly Dictionary<int, Material> Materials = [];
    private static readonly HashSet<int> Drawn = [];
    private static readonly HashSet<int> Reported = [];
    private static float _lastScan = -RescanCooldown;
    private static float _lastMaterialScan = -RescanCooldown;

    public static int Sent(Sprite sprite)
    {
        int hash = Fnv.Hash(sprite.name);
        if (Reported.Add(hash))
        {
            Coop.Current?.Log.LogInfo($"sending sprite {sprite.name} ({hash:x8})");
        }
        return hash;
    }

    public static int Sent(Material? material)
    {
        return material
            ? Fnv.Hash(material!.name.Replace(" (Instance)", ""))
            : 0;
    }

    /// <summary>The material a partner's sprite was drawn with; null keeps the default.</summary>
    public static Material? FindMaterial(int hash)
    {
        if (hash == 0)
        {
            return null;
        }
        if (Materials.TryGetValue(hash, out Material? found) && found)
        {
            return found;
        }
        if (Time.unscaledTime - _lastMaterialScan < RescanCooldown)
        {
            return null;
        }
        _lastMaterialScan = Time.unscaledTime;
        foreach (Material material in Resources.FindObjectsOfTypeAll<Material>())
        {
            Materials[Fnv.Hash(material.name)] = material;
        }
        return Materials.TryGetValue(hash, out found) && found ? found : null;
    }

    /// <summary>Receiver diagnostic: each partner sprite once, as actually drawn here.</summary>
    public static void ReportDrawn(int hash, SpriteRenderer renderer, byte alpha)
    {
        if (Drawn.Add(hash))
        {
            string material = renderer.sharedMaterial ? renderer.sharedMaterial.name : "none";
            string where = $"{renderer.transform.position} scale {renderer.transform.lossyScale}";
            Coop.Current?.Log.LogInfo(
                $"drawing partner sprite {renderer.sprite.name} with {material}, a={alpha}, at {where}");
        }
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
