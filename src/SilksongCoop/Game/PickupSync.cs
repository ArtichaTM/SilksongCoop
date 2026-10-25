using System.Linq;
using HutongGames.PlayMaker;
using SilksongCoop.Net;
using SilksongCoop.Rules;
using UnityEngine;

namespace SilksongCoop.Game;

/// <summary>
/// One pickup per world (9.2): a drop or placed item exists in both games, and whoever takes it first
/// removes the partner's nearest copy. Drops fly out randomly in each game, so copies match by item and
/// distance, not by exact position. Dropped rosaries and shards are not matched: they land differently in
/// each game, so each player collects their own (doubled loot, accepted). Placed ones (saved) stay one-take.
/// </summary>
internal sealed class PickupSync(Coop coop) : Module(coop)
{
    private const float MatchRadius = 8f;

    private enum Kind : byte
    {
        Item,
        Fsm,
        PlacedCurrency,
    }

    public override void Register(Session session) => session.On(MessageType.PickupTaken, OnTaken);

    public void OnLocalItem(CollectableItemPickup pickup) =>
        Report(Kind.Item, ItemKey(pickup), pickup.transform.position);

    public void OnLocalCurrency(CurrencyObjectBase currency)
    {
        if (currency.persistent)
        {
            Report(Kind.PlacedCurrency, PlacedKey(currency), currency.transform.position);
        }
    }

    /// <summary>An FSM pickup (a Mossberry) was taken here; the shared inventory gives it to both.</summary>
    public void OnSwitch(Fsm fsm, FsmState toState)
    {
        GameObject go = fsm.GameObject;
        if (go && SyncedObjects.FsmPickupTaken(go.name, fsm.Name, toState.Name) is { } key)
        {
            Report(Kind.Fsm, key, go.transform.position);
        }
    }

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
        else if (kind == Kind.PlacedCurrency)
        {
            Object.FindObjectsByType<CurrencyObjectBase>(FindObjectsSortMode.None)
                .FirstOrDefault(c => c.persistent && !c.activated && PlacedKey(c) == key)
                ?.Disable(0f);
        }
        else if (kind == Kind.Fsm
            && Nearest<PlayMakerFSM>(at, f => SyncedObjects.IsFsmPickup(f.name, f.FsmName, key)) is { } fsm)
        {
            fsm.gameObject.SetActive(false);
            Coop.Log.LogInfo($"partner took {key}; removed our copy");
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

    private static string PlacedKey(CurrencyObjectBase coin) => EnemySync.IdOf(coin.gameObject).ToString();
}
