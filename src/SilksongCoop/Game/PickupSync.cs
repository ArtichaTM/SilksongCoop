using System.Linq;
using SilksongCoop.Net;
using UnityEngine;

namespace SilksongCoop.Game;

/// <summary>
/// One pickup per world (9.2): a drop or placed item exists in both games, and whoever takes it first
/// removes the partner's nearest copy. Drops fly out randomly in each game, so copies match by item and
/// distance, not by exact position.
/// </summary>
internal sealed class PickupSync(Coop coop) : Module(coop)
{
    private const float MatchRadius = 8f;

    private enum Kind : byte
    {
        Item,
        Currency,
    }

    public override void Register(Session session) => session.On(MessageType.PickupTaken, OnTaken);

    public void OnLocalItem(CollectableItemPickup pickup) =>
        Report(Kind.Item, ItemKey(pickup), pickup.transform.position);

    public void OnLocalCurrency(CurrencyObjectBase currency) =>
        Report(Kind.Currency, CurrencyKey(currency), currency.transform.position);

    private void Report(Kind kind, string key, Vector2 at)
    {
        if (Coop.Rooms.PartnerHere && Coop.Rooms.Current is { } scene)
        {
            Session?.Send(MessageType.PickupTaken, w => w
                .Byte((byte)kind)
                .String(scene)
                .String(key)
                .Float(at.x)
                .Float(at.y));
        }
    }

    private void OnTaken(PacketReader reader)
    {
        Kind kind = (Kind)reader.Byte();
        string scene = reader.String();
        string key = reader.String();
        Vector2 at = new(reader.Float(), reader.Float());
        if (scene != Coop.Rooms.Current)
        {
            return;
        }
        if (kind == Kind.Item)
        {
            CollectableItemPickup? copy = Nearest<CollectableItemPickup>(
                at, p => !p.activatedRead && ItemKey(p) == key);
            RemoveItem(copy, key);
        }
        else if (kind == Kind.Currency
            && Nearest<CurrencyObjectBase>(at, c => !c.activated && CurrencyKey(c) == key) is { } currency)
        {
            currency.Disable(0f);
        }
    }

    /// <summary>Both kneeling for one item at once: the host keeps it, the friend's pickup stops.</summary>
    private void RemoveItem(CollectableItemPickup? pickup, string key)
    {
        if (pickup is null)
        {
            return;
        }
        if (pickup.pickupRoutine != null)
        {
            if (Coop.IsHost)
            {
                return;
            }
            pickup.CancelPickup();
        }
        pickup.activatedRead = true;
        pickup.gameObject.SetActive(false);
        Coop.Log.LogInfo($"partner took {key}; removed our copy");
    }

    private static T? Nearest<T>(Vector2 at, System.Func<T, bool> matches)
        where T : MonoBehaviour
    {
        return Object.FindObjectsByType<T>(FindObjectsSortMode.None)
            .Where(found => found.isActiveAndEnabled && matches(found))
            .Select(found => (Found: found, Distance: Vector2.Distance(at, found.transform.position)))
            .Where(entry => entry.Distance <= MatchRadius)
            .OrderBy(entry => entry.Distance)
            .Select(entry => entry.Found)
            .FirstOrDefault();
    }

    private static string ItemKey(CollectableItemPickup pickup)
    {
        if (pickup.item)
        {
            return pickup.item.name;
        }
        return string.IsNullOrEmpty(pickup.playerDataBool) ? pickup.name : pickup.playerDataBool;
    }

    /// <summary>The coin's type and size: a large rosary is not a small one.</summary>
    private static string CurrencyKey(CurrencyObjectBase currency) =>
        currency.GetType().Name + "/" + currency.name.Replace("(Clone)", "").Trim();
}
