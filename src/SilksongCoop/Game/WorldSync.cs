using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SilksongCoop.Net;
using SilksongCoop.Rules;
using UnityEngine;
using Mutator = SceneData.PersistentMutatorTypes;

namespace SilksongCoop.Game;

/// <summary>
/// One shared world in the host's save (D6, D21, D36a, D36b, D42). Both games diff their
/// <c>PlayerData</c> and <c>SceneData</c> twice a second and send what changed. The host merges the
/// friend's edits (integer counters as deltas, so rosaries and ammo add up) and sends back the result;
/// the friend adopts what the host sends. Loadout and per-player fields stay local (D20).
/// </summary>
internal sealed class WorldSync(Coop coop) : Module(coop)
{
    private const float Interval = 0.5f;
    private const int FloodLimit = 200;
    private const float InventoryCheckInterval = 5f;

    private readonly Dictionary<string, FieldInfo> _fields = SyncedFields();
    private readonly Dictionary<string, Dictionary<string, JToken>> _baseline = [];
    private readonly Dictionary<SceneKey, SceneValue> _sceneBaseline = [];
    private readonly Queue<Action> _early = new();
    private readonly HashSet<string> _inventoryMismatch = [];
    private float _next;
    private float _nextInventoryCheck;
    private PlayerData? _playerData;
    private SceneData? _sceneData;
    private int _friendChangesThisTick;
    private bool _flooded;

    private enum SceneKind : byte
    {
        Bools,
        Ints,
        GeoRocks,
    }

    /// <summary>A baseline exists and diffs are being sent.</summary>
    public bool Running { get; private set; }

    private static JsonSerializer Json
    {
        get
        {
            SaveDataUtility.CreateJsonObjects();
            return SaveDataUtility._serializer;
        }
    }

    public override void Register(Session session)
    {
        session.On(MessageType.FieldChange, OnFieldChange);
        session.On(MessageType.SceneDataChange, OnSceneDataChange);
        session.On(MessageType.InventoryCheck, OnInventoryCheck);
    }

    /// <summary>Takes the current save as the common starting point: the host at the save hand-off, the
    /// friend once the host's save is loaded.</summary>
    public void Begin()
    {
        _playerData = PlayerData.instance;
        _sceneData = SceneData.instance;
        _flooded = false;
        _baseline.Clear();
        foreach ((string name, FieldInfo field) in _fields)
        {
            _baseline[name] = Leaves(name, field);
        }
        _sceneBaseline.Clear();
        foreach ((SceneKey key, SceneValue value) in SceneEntries())
        {
            _sceneBaseline[key] = value;
        }
        Running = true;
        _next = Time.unscaledTime + Interval;
        while (_early.Count > 0)
        {
            _early.Dequeue()();
        }
    }

    public override void Update()
    {
        if (!Running || !Coop.InCoop || Time.unscaledTime < _next)
        {
            return;
        }
        _next = Time.unscaledTime + Interval;
        _friendChangesThisTick = 0;
        if (!SameSave())
        {
            return;
        }
        if (GameRefs.GameplayScene is null)
        {
            // Menus and quit screens hold placeholder data; nothing there is progress.
            return;
        }
        foreach (string name in _fields.Keys)
        {
            SendFieldChanges(name);
        }
        SendSceneChanges();
        if (!Coop.IsHost && Time.unscaledTime >= _nextInventoryCheck)
        {
            _nextInventoryCheck = Time.unscaledTime + InventoryCheckInterval;
            SendInventoryCheck();
        }
    }

    /// <summary>Friend side: hashes of what it carries, so the host can catch a copy that drifted.</summary>
    private void SendInventoryCheck()
    {
        List<string> names = FieldRules.Inventory.Where(_fields.ContainsKey).ToList();
        Session?.Send(MessageType.InventoryCheck, w =>
        {
            w.Int(names.Count);
            foreach (string name in names)
            {
                w.String(name).Int(InventoryHash(name));
            }
        });
    }

    /// <summary>
    /// Host side: a field that differs on two checks in a row is not an edit still in flight; the host
    /// sends its whole value and the friend adopts it.
    /// </summary>
    private void OnInventoryCheck(PacketReader reader)
    {
        int count = reader.Int();
        for (int i = 0; i < count; i++)
        {
            string name = reader.String();
            int friendHash = reader.Int();
            bool differs = Running && _fields.ContainsKey(name) && InventoryHash(name) != friendHash;
            if (!differs)
            {
                _inventoryMismatch.Remove(name);
            }
            else if (!_inventoryMismatch.Add(name))
            {
                _inventoryMismatch.Remove(name);
                Coop.Log.LogWarning($"friend's {name} drifted from the host's; resending it whole");
                string whole = InventoryJson(name);
                Session?.Send(MessageType.FieldChange, w => w.String(name).String("").String(whole));
            }
        }
    }

    private int InventoryHash(string name) => Fnv.Hash(InventoryJson(name));

    private string InventoryJson(string name)
    {
        object? value = _fields[name].GetValue(PlayerData.instance);
        return value is null ? "" : JToken.FromObject(value, Json).ToString(Formatting.None);
    }

    public override void OnLeft()
    {
        Running = false;
        _early.Clear();
    }

    /// <summary>
    /// The game replaces its save objects when it quits to the menu or loads another save. From then on
    /// nothing here is the shared world any more, and diffing it would report a full reset as edits.
    /// </summary>
    private bool SameSave()
    {
        bool same = ReferenceEquals(PlayerData._instance, _playerData)
            && ReferenceEquals(SceneData._instance, _sceneData);
        if (!same && Running)
        {
            Running = false;
            Coop.Log.LogWarning("save data was replaced; world sync stopped");
        }
        return same;
    }

    private static Dictionary<string, FieldInfo> SyncedFields()
    {
        const BindingFlags Instance = BindingFlags.Public | BindingFlags.Instance;
        return typeof(PlayerData).GetFields(Instance)
            .Where(field => !field.IsNotSerialized)
            .Where(field => FieldRules.Of(field.Name) is FieldScope.Shared or FieldScope.HostOwned)
            .ToDictionary(field => field.Name);
    }

    private static Dictionary<string, JToken> Leaves(string name, FieldInfo field)
    {
        object? value = field.GetValue(PlayerData.instance);
        JToken token = value is null ? JValue.CreateNull() : JToken.FromObject(value, Json);
        return JsonLeaves.Flatten(name, token);
    }

    private void SendFieldChanges(string name)
    {
        FieldInfo field = _fields[name];
        Dictionary<string, JToken> now = Leaves(name, field);
        List<LeafChange> changes = JsonLeaves.Diff(_baseline[name], now);
        if (changes.Count == 0)
        {
            return;
        }
        FieldScope scope = FieldRules.Of(name);
        bool sends = Coop.IsHost ? FieldRules.HostSends(scope) : FieldRules.FriendSends(scope);
        if (!sends)
        {
            // D42: the friend's game never moves the host's respawn point; put the host's value back.
            Restore(field, _baseline[name]);
            return;
        }
        _baseline[name] = now;
        foreach (LeafChange change in changes)
        {
            Session?.Send(MessageType.FieldChange, w => w
                .String(change.Path)
                .String(change.Old?.ToString(Formatting.None))
                .String(change.New?.ToString(Formatting.None)));
        }
    }

    private void OnFieldChange(PacketReader reader)
    {
        LeafChange change = new(reader.String(), Parse(reader.String()), Parse(reader.String()));
        if (Coop.IsHost && Flooded())
        {
            return;
        }
        Deliver(() => ApplyField(change));
    }

    /// <summary>Hundreds of edits at once are a reset, not play; the host stops listening.</summary>
    private bool Flooded()
    {
        if (!_flooded && ++_friendChangesThisTick > FloodLimit)
        {
            _flooded = true;
            Coop.Log.LogError("friend sent a burst of save changes; ignoring its changes this session");
            Coop.Hints.Show("Partner's save changes ignored (looked like a reset)", 8f);
        }
        return _flooded;
    }

    /// <summary>The friend keeps what the host sent while its copy of the save was still loading.</summary>
    private void Deliver(Action apply)
    {
        if (Running)
        {
            apply();
        }
        else if (!Coop.IsHost)
        {
            _early.Enqueue(apply);
        }
    }

    private void ApplyField(LeafChange change)
    {
        string path = change.Path;
        string name = JsonLeaves.RootOf(path);
        if (!SameSave() || !_fields.TryGetValue(name, out FieldInfo? field))
        {
            return;
        }
        FieldScope scope = FieldRules.Of(name);
        if (Coop.IsHost && !FieldRules.FriendSends(scope))
        {
            return;
        }
        if (!Coop.IsHost)
        {
            // Our own unsent edit of this field goes out first, so the host can merge it.
            SendFieldChanges(name);
        }
        if (path == name && change.New is null)
        {
            return;
        }
        object? current = field.GetValue(PlayerData.instance);
        JToken? token = current is null ? null : JToken.FromObject(current, Json);
        bool wholeField = path == name;
        if (Coop.IsHost && wholeField && !ProgressGuard.Allows(name, token, change.New))
        {
            Coop.Log.LogWarning($"ignored friend change that undoes progress: {name} -> {change.New}");
            return;
        }
        if (Coop.IsHost && wholeField)
        {
            change = change with { New = ProgressGuard.Merge(name, token, change.New!) };
        }
        JToken? merged = JsonLeaves.Apply(token, change, additive: Coop.IsHost);
        field.SetValue(PlayerData.instance, merged?.ToObject(field.FieldType, Json));
        if (!Coop.IsHost)
        {
            // The host's value is the agreed one; the host itself re-sends its merged result instead.
            _baseline[name] = Leaves(name, field);
        }
    }

    private static void Restore(FieldInfo field, Dictionary<string, JToken> leaves)
    {
        if (leaves.TryGetValue(field.Name, out JToken? whole))
        {
            field.SetValue(PlayerData.instance, whole.ToObject(field.FieldType, Json));
        }
    }

    private static JToken? Parse(string json) => json.Length == 0 ? null : JToken.Parse(json);

    private void SendSceneChanges()
    {
        foreach ((SceneKey key, SceneValue value) in SceneEntries())
        {
            if (_sceneBaseline.TryGetValue(key, out SceneValue known) && known.Equals(value))
            {
                continue;
            }
            _sceneBaseline[key] = value;
            Session?.Send(MessageType.SceneDataChange, w => w
                .Byte((byte)key.Kind)
                .String(key.Scene)
                .String(key.Id)
                .Int(value.Value)
                .Bool(value.SemiPersistent)
                .Byte((byte)value.Mutator));
        }
    }

    private void OnSceneDataChange(PacketReader reader)
    {
        SceneKey key = new((SceneKind)reader.Byte(), reader.String(), reader.String());
        SceneValue value = new(reader.Int(), reader.Bool(), (Mutator)reader.Byte());
        Deliver(() => ApplySceneData(key, value));
    }

    private void ApplySceneData(SceneKey key, SceneValue value)
    {
        if (!SameSave())
        {
            return;
        }
        _sceneBaseline[key] = value;
        SceneData data = SceneData.instance;
        if (key.Kind == SceneKind.Bools)
        {
            data.persistentBools.SetValue(Item(key, value.Value != 0, value));
        }
        else
        {
            SceneData.PersistentIntCollection ints = data.persistentInts;
            if (key.Kind == SceneKind.GeoRocks)
            {
                ints = data.geoRocks;
            }
            ints.SetValue(Item(key, value.Value, value));
        }
    }

    private static PersistentItemData<T> Item<T>(SceneKey key, T stored, SceneValue value)
    {
        return new PersistentItemData<T>
        {
            SceneName = key.Scene,
            ID = key.Id,
            Value = stored,
            IsSemiPersistent = value.SemiPersistent,
            Mutator = value.Mutator,
        };
    }

    private static IEnumerable<(SceneKey Key, SceneValue Value)> SceneEntries()
    {
        SceneData data = SceneData.instance;
        foreach (var entry in Entries(SceneKind.Bools, data.persistentBools.scenes, value => value ? 1 : 0))
        {
            yield return entry;
        }
        foreach (var entry in Entries(SceneKind.Ints, data.persistentInts.scenes, value => value))
        {
            yield return entry;
        }
        foreach (var entry in Entries(SceneKind.GeoRocks, data.geoRocks.scenes, value => value))
        {
            yield return entry;
        }
    }

    private static IEnumerable<(SceneKey Key, SceneValue Value)> Entries<T>(
        SceneKind kind,
        Dictionary<string, Dictionary<string, PersistentItemData<T>>> scenes,
        Func<T, int> number
    )
    {
        foreach (KeyValuePair<string, Dictionary<string, PersistentItemData<T>>> scene in scenes)
        {
            foreach (KeyValuePair<string, PersistentItemData<T>> item in scene.Value)
            {
                PersistentItemData<T> data = item.Value;
                SceneValue value = new(number(data.Value), data.IsSemiPersistent, data.Mutator);
                yield return (new SceneKey(kind, scene.Key, item.Key), value);
            }
        }
    }

    private readonly record struct SceneKey(SceneKind Kind, string Scene, string Id);

    private readonly record struct SceneValue(int Value, bool SemiPersistent, Mutator Mutator);
}
