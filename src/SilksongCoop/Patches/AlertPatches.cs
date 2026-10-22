using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace SilksongCoop.Patches;

/// <summary>
/// H14 for senses: an enemy's alert range and line of sight only track the local hero. In the game that runs
/// the room, they also notice the partner, so enemies wake for and react to either player.
/// </summary>
[HarmonyPatch]
internal static class AlertPatches
{
    private const int Terrain = 256;

    private static readonly Dictionary<int, bool> EnemyRanges = [];

    /// <summary>The partner's Hornet when this game runs the room's enemies and they may notice it.</summary>
    private static Transform? Partner
    {
        get
        {
            if (Coop.Current is not { InCoop: true } coop || !coop.Rooms.PartnerHere || !coop.Rooms.LocalOwns)
            {
                return null;
            }
            return coop.Partner.Downed ? null : coop.Heroes.PartnerTransform;
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(AlertRange), nameof(AlertRange.IsHeroInRange))]
    private static void InRange(AlertRange __instance, ref bool __result)
    {
        if (!__result && Partner is { } partner && IsEnemys(__instance))
        {
            __result = Senses(__instance, partner.position);
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(LineOfSightDetector), "Update")]
    private static void Sight(LineOfSightDetector __instance)
    {
        if (__instance.canSeeHero || Partner is not { } partner)
        {
            return;
        }
        Vector2 from = __instance.transform.position;
        Vector2 to = partner.position;
        bool sensed = __instance.alertRanges.Length == 0;
        foreach (AlertRange range in __instance.alertRanges)
        {
            sensed |= range && Senses(range, to);
        }
        __instance.canSeeHero = sensed && !Helper.LineCast2DHit(from, to, Terrain, out _);
    }

    /// <summary>Inside one of the range's triggers, and seen from where the range looks.</summary>
    private static bool Senses(AlertRange range, Vector2 at)
    {
        bool inside = false;
        foreach (Collider2D trigger in range.GetComponents<Collider2D>())
        {
            inside |= trigger && trigger.enabled && trigger.OverlapPoint(at);
        }
        if (!inside || !range.ChecksLineOfSight)
        {
            return inside;
        }
        Transform eye = range.lineOfSight == AlertRange.LineOfSightChecks.Self
            ? range.transform
            : range.transform.parent ? range.transform.parent : range.initialParent;
        return eye && !Helper.LineCast2DHit(eye.position, at, Terrain, out _);
    }

    /// <summary>Alert ranges also drive non-enemies (the magnet tool pulling rosaries to the hero).</summary>
    private static bool IsEnemys(AlertRange range)
    {
        int key = range.GetInstanceID();
        if (!EnemyRanges.TryGetValue(key, out bool enemy))
        {
            enemy = range.GetComponentInParent<HealthManager>(true);
            EnemyRanges[key] = enemy;
        }
        return enemy;
    }
}
