using HarmonyLib;
using UnityEngine;

namespace SilksongCoop.Patches;

/// <summary>Claims what the local hero starts to pick up; the partner's copy goes before the grant.</summary>
[HarmonyPatch]
internal static class PickupPatches
{
    /// <summary>The kneel takes 0.75 s before the item is granted; the claim goes out as it starts.</summary>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(CollectableItemPickup), nameof(CollectableItemPickup.DoPickup))]
    private static void ItemStarted(CollectableItemPickup __instance) => Claim(__instance);

    [HarmonyPrefix]
    [HarmonyPatch(typeof(CollectableItemPickup), nameof(CollectableItemPickup.DoPickupInstant))]
    private static void ItemInstant(CollectableItemPickup __instance) => Claim(__instance);

    [HarmonyPostfix]
    [HarmonyPatch(typeof(CurrencyObjectBase), nameof(CurrencyObjectBase.Collect))]
    private static void CurrencyTaken(CurrencyObjectBase __instance, bool __result)
    {
        if (__result)
        {
            Coop.Current?.Pickups.OnLocalCurrency(__instance);
        }
    }

    /// <summary>The same checks the game makes before it starts a pickup.</summary>
    private static void Claim(CollectableItemPickup pickup)
    {
        if (!pickup.activatedRead && Time.timeAsDouble >= pickup.canPickupTime)
        {
            Coop.Current?.Pickups.OnLocalItem(pickup);
        }
    }
}

/// <summary>Every rosary or shard spent, for the unsynced-purchase log (user note 1).</summary>
[HarmonyPatch(typeof(CurrencyManager), nameof(CurrencyManager.ChangeCurrency))]
internal static class SpendPatches
{
    private static void Prefix(int amount, CurrencyType type)
    {
        if (amount < 0)
        {
            Coop.Current?.Spending.OnSpent(-amount, type);
        }
    }
}

/// <summary>A purchase closes the partner's open shop (3.4 a).</summary>
[HarmonyPatch(typeof(ShopItem), nameof(ShopItem.SetPurchased))]
internal static class ShopPatches
{
    private static void Postfix() => Coop.Current?.Shops.OnLocalPurchase();
}
