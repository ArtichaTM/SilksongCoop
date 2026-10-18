using System.Collections.Generic;
using System.Linq;
using HutongGames.PlayMaker;
using SilksongCoop.Net;
using SilksongCoop.Rules;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SilksongCoop.Game;

internal enum FsmMode
{
    /// <summary>Not synced: hero, UI, NPC dialogue (D30 keeps dialogue local).</summary>
    None,

    /// <summary>Enemies and boss arenas: the room owner runs them, the other game mirrors (D9).</summary>
    Authority,

    /// <summary>Levers, gates, lifts (D46): either player may trigger them; both games follow.</summary>
    Shared,
}

/// <summary>
/// Enemies and room objects with two players in one room. The owner sends positions, HP and FSM state
/// switches; the mirror's FSMs take no events of their own, its bodies turn kinematic (no fighting the
/// level geometry, R4) and it only dies when told. Hits are judged by the hitter (D35) and forwarded.
/// </summary>
internal sealed class EnemySync : Module
{
    private const float FrameInterval = 1f / 30f;
    private const float HideUnseenAfter = 2f;
    private const float SnapDistance = 4f;
    private const float FollowRate = 25f;
    private const int EntriesPerPacket = 48;
    private const int SnapshotChunk = 16;
    private const float ReindexCooldown = 1f;

    private static readonly string[] SharedKeywords =
    [
        "Lever", "Gate", "Lift", "Elevator", "Platform", "Bridge",
    ];

    private readonly Dictionary<int, Tracked> _enemies = [];
    private readonly Dictionary<Fsm, FsmMode> _modes = [];
    private readonly Dictionary<int, GameObject> _objects = [];
    private readonly Dictionary<Fsm, (string State, string Event)> _relayed = [];
    private float _nextFrame;
    private float _lastIndex = -ReindexCooldown;
    private float _enteredAt;
    private GameObject? _hitSource;

    public EnemySync(Coop coop)
        : base(coop) => coop.Rooms.Entered += OnEntered;

    /// <summary>Applying a state the partner sent; patches must not echo it back.</summary>
    public static bool Applying { get; private set; }

    /// <summary>Inside a local hit on a mirrored enemy: the one death a mirror may run itself.</summary>
    public static bool LocalHit { get; set; }

    /// <summary>Inside a death the partner reported.</summary>
    public static bool RemoteDeath { get; private set; }

    private bool Mirroring => !Coop.Rooms.LocalOwns;

    public override void Register(Session session)
    {
        session.On(MessageType.EnemyFrame, OnFrame);
        session.On(MessageType.FsmState, OnFsmState);
        session.On(MessageType.FsmEventRelay, OnRelayedEvent);
        session.On(MessageType.EnemyHit, OnHit);
        session.On(MessageType.EnemyDied, OnDied);
        session.On(MessageType.RoomSnapshot, OnSnapshot);
        session.On(MessageType.BattleStart, OnBattleStart);
    }

    public override void Update()
    {
        if (Coop.Rooms.Current is null)
        {
            return;
        }
        bool mirroring = Mirroring;
        float now = Time.unscaledTime;
        foreach (Tracked enemy in _enemies.Values)
        {
            if (!enemy.Health)
            {
                continue;
            }
            enemy.SetMirrored(mirroring);
            if (mirroring)
            {
                Follow(enemy, now);
            }
        }
        if (!mirroring && Coop.Rooms.PartnerHere && now >= _nextFrame)
        {
            _nextFrame = now + FrameInterval;
            SendFrame();
        }
    }

    public override void OnLeft()
    {
        foreach (Tracked enemy in _enemies.Values)
        {
            enemy.SetMirrored(false);
        }
    }

    public override void Dispose()
    {
        OnLeft();
        Coop.Rooms.Entered -= OnEntered;
        if (_hitSource)
        {
            Object.Destroy(_hitSource);
        }
    }

    public FsmMode ModeOf(Fsm fsm)
    {
        if (_modes.TryGetValue(fsm, out FsmMode mode))
        {
            return mode;
        }
        mode = Classify(fsm);
        _modes[fsm] = mode;
        if (mode != FsmMode.None)
        {
            _objects[IdOf(fsm.GameObject)] = fsm.GameObject;
        }
        return mode;
    }

    /// <summary>Owner side: a synced FSM is about to switch state; the mirror follows.</summary>
    public void OnSwitch(Fsm fsm, FsmState toState)
    {
        if (Applying || !Coop.Rooms.PartnerHere)
        {
            return;
        }
        FsmMode mode = ModeOf(fsm);
        bool send = mode == FsmMode.Shared || (mode == FsmMode.Authority && Coop.Rooms.LocalOwns);
        if (send)
        {
            int id = IdOf(fsm.GameObject);
            Session?.Send(MessageType.FsmState, w => w.Int(id).String(fsm.Name).String(toState.Name));
        }
    }

    /// <summary>A mirrored FSM only moves when the owner says so, even inside an applied state.</summary>
    public bool BlocksEvent(Fsm fsm) => Mirroring && ModeOf(fsm) == FsmMode.Authority;

    /// <summary>
    /// A mirror reacted to something only it can see (its own player nearby): the owner takes the same
    /// event if it is in the same state. Once per state entry; plain FINISHED stays the owner's call.
    /// </summary>
    public void RelayBlocked(Fsm fsm, FsmEvent fsmEvent)
    {
        if (fsm.ActiveState is not { } state || fsmEvent == FsmEvent.Finished || !Leads(fsm, state, fsmEvent))
        {
            return;
        }
        (string, string) relay = (state.Name, fsmEvent.Name);
        if (_relayed.TryGetValue(fsm, out (string, string) last) && last == relay)
        {
            return;
        }
        _relayed[fsm] = relay;
        int id = IdOf(fsm.GameObject);
        Session?.Send(MessageType.FsmEventRelay, w => w
            .Int(id)
            .String(fsm.Name)
            .String(state.Name)
            .String(fsmEvent.Name));
    }

    private static bool Leads(Fsm fsm, FsmState state, FsmEvent fsmEvent)
    {
        return state.Transitions.Any(t => t.FsmEvent == fsmEvent)
            || fsm.GlobalTransitions.Any(t => t.FsmEvent == fsmEvent);
    }

    public bool IsMirrored(HealthManager health) => !Applying && Mirroring && IsTracked(health);

    private bool IsTracked(HealthManager health) => _enemies.ContainsKey(IdOf(health.gameObject));

    public bool IsOwnedAndShared(HealthManager health)
    {
        bool owned = Coop.Rooms.LocalOwns && Coop.Rooms.PartnerHere;
        return owned && IsTracked(health);
    }

    /// <summary>A local hit on a mirrored enemy: the owner applies the same damage (D35).</summary>
    public void ReportHit(HealthManager health, int damage, HitInstance hit)
    {
        int id = IdOf(health.gameObject);
        bool killed = health.isDead;
        Session?.Send(MessageType.EnemyHit, w => w
            .Int(id)
            .Int(damage)
            .Float(hit.Direction)
            .Int((int)hit.AttackType)
            .Bool(killed));
    }

    public void ReportDeath(HealthManager health)
    {
        int id = IdOf(health.gameObject);
        Session?.Send(MessageType.EnemyDied, w => w.Int(id));
    }

    public void Track(HealthManager health)
    {
        if (Coop.Rooms.Current is null || health.gameObject.scene.name != Coop.Rooms.Current)
        {
            return;
        }
        int id = IdOf(health.gameObject);
        if (!_enemies.TryGetValue(id, out Tracked? known) || known.Health != health)
        {
            _enemies[id] = new Tracked(id, health) { LastSeen = Time.unscaledTime };
        }
    }

    public void ReportBattleStart(BattleScene battle)
    {
        if (!Applying && Coop.Rooms.PartnerHere)
        {
            int id = IdOf(battle.gameObject);
            Session?.Send(MessageType.BattleStart, w => w.Int(id));
        }
    }

    /// <summary>Sent by the game that was in the room first when the partner arrives (D12).</summary>
    public void SendSnapshot()
    {
        if (!Coop.InCoop || Coop.Rooms.Current is null)
        {
            return;
        }
        int room = Coop.Rooms.CurrentHash;
        List<Tracked> alive = _enemies.Values.Where(enemy => enemy.Health).ToList();
        for (int start = 0; start < alive.Count; start += SnapshotChunk)
        {
            List<Tracked> chunk = alive.Skip(start).Take(SnapshotChunk).ToList();
            Session?.Send(MessageType.RoomSnapshot, w =>
            {
                w.Int(room).Int(chunk.Count);
                foreach (Tracked enemy in chunk)
                {
                    WriteSnapshot(w, enemy);
                }
            });
        }
    }

    private static int IdOf(GameObject go) => Fnv.Hash(go.scene.name + "/" + GameRefs.PathOf(go.transform));

    private void OnEntered(string scene)
    {
        _enemies.Clear();
        _modes.Clear();
        _objects.Clear();
        _relayed.Clear();
        _enteredAt = Time.unscaledTime;
        foreach (HealthManager health in Resources.FindObjectsOfTypeAll<HealthManager>())
        {
            if (health.gameObject.scene.name == scene)
            {
                Track(health);
            }
        }
    }

    private FsmMode Classify(Fsm fsm)
    {
        GameObject go = fsm.GameObject;
        if (!go || go.scene.name != Coop.Rooms.Current || go.GetComponentInParent<HeroController>(true))
        {
            return FsmMode.None;
        }
        if (go.GetComponentInParent<HealthManager>(true) || InBossArena(go.transform))
        {
            return FsmMode.Authority;
        }
        bool persistent = go.GetComponent<PersistentBoolItem>() || go.GetComponent<PersistentIntItem>();
        bool named = fsm.Name == "BG Control" || SharedKeywords.Any(word => fsm.Name.Contains(word));
        return persistent || named ? FsmMode.Shared : FsmMode.None;
    }

    private static bool InBossArena(Transform transform)
    {
        for (Transform? at = transform; at != null; at = at.parent)
        {
            if (at.name.StartsWith("Boss Scene"))
            {
                return true;
            }
        }
        return false;
    }

    private void Follow(Tracked enemy, float now)
    {
        GameObject go = enemy.Health.gameObject;
        if (enemy.HasTarget && go.activeSelf)
        {
            Transform transform = go.transform;
            Vector2 current = transform.position;
            float blend = 1f - Mathf.Exp(-FollowRate * Time.deltaTime);
            bool far = Vector2.Distance(current, enemy.Target) > SnapDistance;
            Vector2 next = far ? enemy.Target : Vector2.Lerp(current, enemy.Target, blend);
            transform.position = new Vector3(next.x, next.y, transform.position.z);
            Vector3 scale = transform.localScale;
            transform.localScale = new Vector3(enemy.ScaleX, scale.y, scale.z);
        }
        bool unseen = now - enemy.LastSeen > HideUnseenAfter && now - _enteredAt > HideUnseenAfter;
        if (unseen && Coop.Rooms.PartnerHere && go.activeSelf && !enemy.Health.isDead)
        {
            // The owner doesn't list it: not spawned there, or already gone.
            go.SetActive(false);
        }
    }

    private void SendFrame()
    {
        int room = Coop.Rooms.CurrentHash;
        List<Tracked> active = _enemies.Values
            .Where(enemy => enemy.Health && enemy.Health.gameObject.activeInHierarchy && !enemy.Health.isDead)
            .ToList();
        for (int start = 0; start < active.Count; start += EntriesPerPacket)
        {
            List<Tracked> chunk = active.Skip(start).Take(EntriesPerPacket).ToList();
            Session?.SendFast(MessageType.EnemyFrame, w =>
            {
                w.Int(room).Byte((byte)chunk.Count);
                foreach (Tracked enemy in chunk)
                {
                    Transform transform = enemy.Health.transform;
                    w.Int(enemy.Id)
                        .Float(transform.position.x)
                        .Float(transform.position.y)
                        .Float(transform.localScale.x)
                        .Int(enemy.Health.hp);
                }
            });
        }
    }

    private void OnFrame(PacketReader reader)
    {
        bool forThisRoom = reader.Int() == Coop.Rooms.CurrentHash;
        int count = reader.Byte();
        for (int i = 0; i < count; i++)
        {
            int id = reader.Int();
            Vector2 position = new(reader.Float(), reader.Float());
            float scaleX = reader.Float();
            int hp = reader.Int();
            if (!forThisRoom || !Mirroring || !_enemies.TryGetValue(id, out Tracked? enemy) || !enemy.Health)
            {
                continue;
            }
            enemy.Target = position;
            enemy.ScaleX = scaleX;
            enemy.HasTarget = true;
            enemy.LastSeen = Time.unscaledTime;
            HealthManager health = enemy.Health;
            if (!health.isDead)
            {
                health.hp = hp;
                if (!health.gameObject.activeSelf)
                {
                    health.gameObject.SetActive(true);
                }
            }
        }
    }

    private void OnFsmState(PacketReader reader)
    {
        int id = reader.Int();
        string fsmName = reader.String();
        string state = reader.String();
        PlayMakerFSM? component = FindFsm(id, fsmName);
        if (component is null)
        {
            return;
        }
        FsmMode mode = ModeOf(component.Fsm);
        bool apply = mode == FsmMode.Shared ? component.ActiveStateName != state : Mirroring;
        if (apply && mode != FsmMode.None)
        {
            SetState(component, state);
        }
    }

    private void OnRelayedEvent(PacketReader reader)
    {
        int id = reader.Int();
        string fsmName = reader.String();
        string state = reader.String();
        string eventName = reader.String();
        if (Mirroring || FindFsm(id, fsmName) is not { } component || component.ActiveStateName != state)
        {
            return;
        }
        if (ModeOf(component.Fsm) == FsmMode.Authority)
        {
            component.SendEvent(eventName);
        }
    }

    private void OnHit(PacketReader reader)
    {
        int id = reader.Int();
        int damage = reader.Int();
        float direction = reader.Float();
        AttackTypes attack = (AttackTypes)reader.Int();
        bool killed = reader.Bool();
        if (Mirroring || !_enemies.TryGetValue(id, out Tracked? enemy) || !enemy.Health)
        {
            return;
        }
        HealthManager health = enemy.Health;
        if (health.isDead)
        {
            return;
        }
        if (killed)
        {
            // The hitter's game already dropped the loot for the killing blow.
            DieQuietly(health, direction, attack);
            return;
        }
        int expected = health.hp - damage;
        HitInstance hit = new()
        {
            Source = HitSource(),
            AttackType = attack,
            DamageDealt = damage,
            Direction = direction,
            Multiplier = 1f,
            MagnitudeMultiplier = 1f,
            SilkGeneration = HitSilkGeneration.None,
        };
        health.Hit(hit);
        if (!health.isDead && health.hp > expected)
        {
            health.hp = expected;
            if (expected <= 0)
            {
                health.Die(direction, attack, true);
            }
        }
    }

    private void OnDied(PacketReader reader)
    {
        int id = reader.Int();
        if (Mirroring && _enemies.TryGetValue(id, out Tracked? enemy) && enemy.Health && !enemy.Health.isDead)
        {
            DieQuietly(enemy.Health, null, AttackTypes.Generic);
        }
    }

    private void OnBattleStart(PacketReader reader)
    {
        int id = reader.Int();
        BattleScene? battle = Object.FindObjectsByType<BattleScene>(FindObjectsSortMode.None)
            .FirstOrDefault(b => IdOf(b.gameObject) == id);
        if (battle is null || battle.started)
        {
            return;
        }
        Applying = true;
        try
        {
            battle.StartBattle();
        }
        finally
        {
            Applying = false;
        }
    }

    private void WriteSnapshot(PacketWriter writer, Tracked enemy)
    {
        HealthManager health = enemy.Health;
        Transform transform = health.transform;
        List<PlayMakerFSM> fsms = health.GetComponentsInChildren<PlayMakerFSM>(true).ToList();
        writer.Int(enemy.Id)
            .Bool(health.isDead)
            .Bool(health.gameObject.activeSelf)
            .Int(health.hp)
            .Float(transform.position.x)
            .Float(transform.position.y)
            .Float(transform.localScale.x)
            .Int(fsms.Count);
        foreach (PlayMakerFSM fsm in fsms)
        {
            writer.Int(IdOf(fsm.gameObject)).String(fsm.FsmName).String(fsm.ActiveStateName ?? "");
        }
    }

    private void OnSnapshot(PacketReader reader)
    {
        bool forThisRoom = reader.Int() == Coop.Rooms.CurrentHash;
        int count = reader.Int();
        for (int i = 0; i < count; i++)
        {
            int id = reader.Int();
            bool dead = reader.Bool();
            bool active = reader.Bool();
            int hp = reader.Int();
            Vector2 position = new(reader.Float(), reader.Float());
            float scaleX = reader.Float();
            List<(int Object, string Fsm, string State)> states = [];
            int fsmCount = reader.Int();
            for (int f = 0; f < fsmCount; f++)
            {
                states.Add((reader.Int(), reader.String(), reader.String()));
            }
            if (forThisRoom && _enemies.TryGetValue(id, out Tracked? enemy) && enemy.Health)
            {
                ApplySnapshot(enemy, dead, active, hp, position, scaleX);
                if (!Coop.IsHost)
                {
                    // The host takes over and restarts behaviour (D12); a mirror copies it.
                    states.ForEach(entry => SetState(FindFsm(entry.Object, entry.Fsm), entry.State));
                }
            }
        }
    }

    private void ApplySnapshot(Tracked enemy, bool dead, bool active, int hp, Vector2 position, float scaleX)
    {
        HealthManager health = enemy.Health;
        GameObject go = health.gameObject;
        if (dead)
        {
            health.isDead = true;
            go.SetActive(false);
            return;
        }
        health.hp = hp;
        Transform transform = go.transform;
        transform.position = new Vector3(position.x, position.y, transform.position.z);
        Vector3 scale = transform.localScale;
        transform.localScale = new Vector3(scaleX, scale.y, scale.z);
        enemy.Target = position;
        enemy.ScaleX = scaleX;
        enemy.HasTarget = true;
        enemy.LastSeen = Time.unscaledTime;
        if (go.activeSelf != active)
        {
            go.SetActive(active);
        }
    }

    private void SetState(PlayMakerFSM? component, string state)
    {
        if (component is null || state.Length == 0 || !component.Fsm.HasState(state))
        {
            return;
        }
        _relayed.Remove(component.Fsm);
        Applying = true;
        try
        {
            component.SetState(state);
        }
        finally
        {
            Applying = false;
        }
    }

    private static void DieQuietly(HealthManager health, float? direction, AttackTypes attack)
    {
        RemoteDeath = true;
        try
        {
            health.Die(direction, attack, NailElements.None, null, true, 1f, false, true);
        }
        finally
        {
            RemoteDeath = false;
        }
    }

    private PlayMakerFSM? FindFsm(int id, string fsmName)
    {
        if (!_objects.TryGetValue(id, out GameObject? go) || !go)
        {
            Reindex();
            if (!_objects.TryGetValue(id, out go) || !go)
            {
                return null;
            }
        }
        return go.GetComponents<PlayMakerFSM>().FirstOrDefault(fsm => fsm.FsmName == fsmName);
    }

    private void Reindex()
    {
        if (Time.unscaledTime - _lastIndex < ReindexCooldown || Coop.Rooms.Current is not { } scene)
        {
            return;
        }
        _lastIndex = Time.unscaledTime;
        Scene active = SceneManager.GetSceneByName(scene);
        foreach (PlayMakerFSM fsm in Resources.FindObjectsOfTypeAll<PlayMakerFSM>())
        {
            if (fsm.gameObject.scene == active)
            {
                _objects[IdOf(fsm.gameObject)] = fsm.gameObject;
            }
        }
    }

    private GameObject HitSource()
    {
        if (!_hitSource)
        {
            _hitSource = new GameObject("SilksongCoop Partner Hit");
            Object.DontDestroyOnLoad(_hitSource);
        }
        return _hitSource!;
    }

    private sealed class Tracked(int id, HealthManager health)
    {
        private RigidbodyType2D? _ownBodyType;

        public int Id { get; } = id;

        public HealthManager Health { get; } = health;

        public Vector2 Target { get; set; }

        public float ScaleX { get; set; } = 1f;

        public bool HasTarget { get; set; }

        public float LastSeen { get; set; }

        /// <summary>A mirror's body is kinematic: its physics never fights the owner's positions.</summary>
        public void SetMirrored(bool mirrored)
        {
            if (!Health || !Health.TryGetComponent(out Rigidbody2D body))
            {
                return;
            }
            if (mirrored && _ownBodyType is null)
            {
                _ownBodyType = body.bodyType;
                body.bodyType = RigidbodyType2D.Kinematic;
                body.linearVelocity = Vector2.zero;
            }
            else if (!mirrored && _ownBodyType is { } own)
            {
                body.bodyType = own;
                _ownBodyType = null;
            }
        }
    }
}
