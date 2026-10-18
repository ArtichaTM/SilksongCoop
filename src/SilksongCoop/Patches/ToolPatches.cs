using HarmonyLib;
using SilksongCoop.Game;
using UnityEngine;

namespace SilksongCoop.Patches;

/// <summary>Thrown tools on both games (D45): spawns get reported, partner copies do no damage.</summary>
[HarmonyPatch]
internal static class ToolPatches
{
    private static bool InCoop => Coop.Current is { InCoop: true };

    [HarmonyPrefix]
    [HarmonyPatch(typeof(HeroController), "ThrowTool")]
    private static void ThrowBefore() => ToolReplicas.BeginThrow();

    [HarmonyFinalizer]
    [HarmonyPatch(typeof(HeroController), "ThrowTool")]
    private static void ThrowAfter() => ToolReplicas.EndThrow();

    [HarmonyPostfix]
    [HarmonyPatch(
        typeof(ObjectPool),
        nameof(ObjectPool.Spawn),
        [typeof(GameObject), typeof(Transform), typeof(Vector3), typeof(Quaternion), typeof(bool)]
    )]
    private static void Spawned(GameObject prefab, GameObject __result)
    {
        if (InCoop && prefab && __result)
        {
            ToolReplicas.OnSpawned(prefab, __result);
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ObjectPool), nameof(ObjectPool.Recycle), [typeof(GameObject)])]
    private static void Recycled(GameObject obj)
    {
        if (obj)
        {
            ToolReplicas.OnRecycled(obj);
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(DamageEnemies), nameof(DamageEnemies.DoDamage), [typeof(GameObject), typeof(bool)])]
    private static bool Damage(DamageEnemies __instance, ref bool __result)
    {
        if (!InCoop || !ToolReplicas.IsReplica(__instance))
        {
            return true;
        }
        __result = false;
        return false;
    }
}
