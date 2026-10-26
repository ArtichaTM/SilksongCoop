using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using SilksongCoop.Net;
using UnityEngine;

namespace SilksongCoop.Game;

/// <summary>
/// Saved room objects (rosary strings' hits left, cocoons, cut platforms, walls): the game writes their state
/// into the save only when a room is left, so a partner arriving in a room the owner changed loads the old
/// state. The owner sends each object's current value with the arrival snapshot, and the arriving game
/// replays it the way a room load does (the finished result, no effects).
/// </summary>
internal sealed class PersistentSync(Coop coop) : Module(coop)
{
    private const int EntriesPerPacket = 32;

    private static readonly AccessTools.FieldRef<PersistentItem<bool>, PersistentItem<bool>.SetValueEvent?>
        BoolLoaded = AccessTools.FieldRefAccess<PersistentItem<bool>, PersistentItem<bool>.SetValueEvent?>(
            nameof(PersistentItem<bool>.OnSetSaveState));

    private static readonly AccessTools.FieldRef<PersistentItem<int>, PersistentItem<int>.SetValueEvent?>
        IntLoaded = AccessTools.FieldRefAccess<PersistentItem<int>, PersistentItem<int>.SetValueEvent?>(
            nameof(PersistentItem<int>.OnSetSaveState));

    public override void Register(Session session) => session.On(MessageType.PersistentSnapshot, OnSnapshot);

    /// <summary>Called by the game that runs the room when the partner arrives.</summary>
    public void SendSnapshot()
    {
        List<(int Id, int Value)> entries =
        [
            .. Items<PersistentBoolItem>().Select(item => Entry(item, item.GetCurrentValue() ? 1 : 0)),
            .. Items<PersistentIntItem>().Select(item => Entry(item, item.GetCurrentValue())),
        ];
        int room = Coop.Rooms.CurrentHash;
        for (int start = 0; start < entries.Count; start += EntriesPerPacket)
        {
            List<(int Id, int Value)> chunk = entries.Skip(start).Take(EntriesPerPacket).ToList();
            Session?.Send(MessageType.PersistentSnapshot, w =>
            {
                w.Int(room).Int(chunk.Count);
                chunk.ForEach(entry => w.Int(entry.Id).Int(entry.Value));
            });
        }
    }

    private static (int Id, int Value) Entry(Component item, int value) =>
        (EnemySync.IdOf(item.gameObject), value);

    private IEnumerable<T> Items<T>()
        where T : MonoBehaviour
    {
        return Object.FindObjectsByType<T>(FindObjectsSortMode.None)
            .Where(item => Coop.Rooms.Holds(item.gameObject.scene));
    }

    private void OnSnapshot(PacketReader reader)
    {
        bool forThisRoom = reader.Int() == Coop.Rooms.CurrentHash;
        int count = reader.Int();
        Dictionary<int, int> values = [];
        for (int i = 0; i < count; i++)
        {
            values[reader.Int()] = reader.Int();
        }
        if (!forThisRoom)
        {
            return;
        }
        foreach (PersistentBoolItem item in Items<PersistentBoolItem>())
        {
            if (values.TryGetValue(EnemySync.IdOf(item.gameObject), out int value))
            {
                Load(item, value != 0, BoolLoaded(item));
            }
        }
        foreach (PersistentIntItem item in Items<PersistentIntItem>())
        {
            if (values.TryGetValue(EnemySync.IdOf(item.gameObject), out int value))
            {
                Load(item, value, IntLoaded(item));
            }
        }
    }

    /// <summary>Sets a saved bool now, so leaving the room saves it too.</summary>
    public static void Set(PersistentBoolItem item, bool value) => Load(item, value, BoolLoaded(item));

    /// <summary>What <c>PersistentItem.Start</c> does with a saved value.</summary>
    private static void Load<T>(PersistentItem<T> item, T value, PersistentItem<T>.SetValueEvent? loaded)
        where T : System.IEquatable<T>
    {
        if (item.GetCurrentValue().Equals(value))
        {
            return;
        }
        item.ItemData.Value = value;
        loaded?.Invoke(value);
        if (item.fsm)
        {
            item.SetValueOnFSM(item.fsm, value);
        }
    }
}
