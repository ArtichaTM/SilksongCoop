using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using SilksongCoop.Game;

namespace SilksongCoop.Patches;

/// <summary>Hits on room objects (1.3 a): a hit the game took stamps its time, a filtered one not.</summary>
[HarmonyPatch(typeof(HitResponse), nameof(HitResponse.Hit))]
internal static class HitResponsePatches
{
    private static void Prefix(HitResponse __instance, out double __state) =>
        __state = __instance.lastHitTime;

    private static void Postfix(HitResponse __instance, HitInstance damageInstance, double __state)
    {
        if (__instance.lastHitTime != __state && Coop.Current is { InCoop: true } coop)
        {
            coop.HitResponses.OnLocalHit(__instance, damageInstance);
        }
    }
}

/// <summary>A replayed hit (bouncy object, lever) must not knock the local hero back.</summary>
[HarmonyPatch]
internal static class ReplayRecoilPatches
{
    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(HeroController), nameof(HeroController.RecoilLeft));
        yield return AccessTools.Method(typeof(HeroController), nameof(HeroController.RecoilRight));
        yield return AccessTools.Method(typeof(HeroController), nameof(HeroController.RecoilDown));
    }

    private static bool Prefix() => !HitResponseSync.Applying && !LeverSync.Applying;
}
