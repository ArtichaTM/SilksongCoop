using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using SilksongCoop.Game;
using UnityEngine;

namespace SilksongCoop.Patches;

/// <summary>
/// H14 for senses: an enemy's alert range and line of sight only track the local hero. In both games they
/// also notice the partner, so enemies wake for and react to either player. The mirror senses both too:
/// sensing only its own player, it relayed "out of range" while the owner's player stood in range, and the
/// enemy flipped between two states every frame (Skarr hunters, Bone Circler).
/// </summary>
[HarmonyPatch]
internal static class AlertPatches
{
    private const int Terrain = 256;
    private const float BodyRadius = 0.7f;

    private const float TrustFor = 1f;

    private static readonly Dictionary<int, bool> EnemyRanges = [];
    private static readonly Dictionary<int, float> Trusted = [];

    /// <summary>The partner's Hornet when it shares the room and enemies may notice it.</summary>
    private static Transform? Partner
    {
        get
        {
            if (Coop.Current is not { InCoop: true } coop || !coop.Rooms.PartnerHere)
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
            __result = Senses(__instance, partner.position) || IsTrusted(__instance);
        }
    }

    /// <summary>
    /// The partner's game just saw its hero in this enemy's range (a relayed event). The partner's drawn
    /// position here lags by the ping, so the enemy's next range check believes it for a moment instead of
    /// dropping the chase it just started (10.05: the Bone Hunter Tiny never kept chasing the partner).
    /// </summary>
    public static void TrustPartner(GameObject enemy)
    {
        float until = Time.time + TrustFor;
        foreach (AlertRange range in enemy.GetComponentsInChildren<AlertRange>(true))
        {
            Trusted[range.GetInstanceID()] = until;
        }
    }

    private static bool IsTrusted(AlertRange range) =>
        Trusted.TryGetValue(range.GetInstanceID(), out float until) && Time.time < until;

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

    /// <summary>
    /// Inside one of the range's triggers, and seen from where the range looks. The partner's game senses
    /// its whole body, not a point: judged by the point alone, an edge the mirror relays as "in range" reads
    /// "out" here, and the enemy flips between chase and walk every frame (marks 11-12).
    /// </summary>
    private static bool Senses(AlertRange range, Vector2 at)
    {
        bool inside = false;
        foreach (Collider2D trigger in range.GetComponents<Collider2D>())
        {
            if (trigger && trigger.enabled)
            {
                inside |= Vector2.Distance(trigger.ClosestPoint(at), at) <= BodyRadius;
            }
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

    /// <summary>
    /// Alert ranges also drive the hero's own things (the magnet tool pulling rosaries to it); only enemies
    /// and synced room objects (a flea barrel's rescue range, 3.3) sense the partner. An enemy may detach its
    /// range on start (Bone Hunter's unalert range), so the range's first parent decides.
    /// </summary>
    private static bool IsEnemys(AlertRange range)
    {
        int key = range.GetInstanceID();
        if (!EnemyRanges.TryGetValue(key, out bool enemy))
        {
            Transform owner = range.initialParent ? range.initialParent : range.transform;
            enemy = owner.GetComponentInParent<HealthManager>(true) || IsSyncedObject(owner);
            EnemyRanges[key] = enemy;
        }
        return enemy;
    }

    private static bool IsSyncedObject(Transform owner)
    {
        PlayMakerFSM[] fsms = owner.GetComponentsInParent<PlayMakerFSM>(true);
        return Coop.Current is { } coop && fsms.Any(fsm => coop.Enemies.ModeOf(fsm.Fsm) != FsmMode.None);
    }
}
