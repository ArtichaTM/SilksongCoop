using System.Collections.Generic;
using System.Linq;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using SilksongCoop.Net;
using SilksongCoop.Patches;
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

    /// <summary>
    /// Levers, gates, walls, lifts (D46): either player may trigger them. The room owner runs them; the other
    /// game relays its triggers and mirrors, so a chain of states never plays twice.
    /// </summary>
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
    private const int MaxReports = 60;
    private const int StartChainDepth = 8;

    // Cages, branches, barrels and sleeping fleas that a few hits break open (no save flag of their own).
    private const string FleaRescue = "Flea Rescue";

    // What HealthManager.Die sends: a death is the owner's call, a mirror never relays it (mark 16-19 loop).
    private static readonly HashSet<string> DeathEvents = ["ZERO HP", "FATAL DAMAGE", "LAVA DEATH"];

    private static readonly string[] SharedKeywords =
    [
        "Lever", "Gate", "Lift", "Elevator", "Platform", "Bridge",
    ];

    // Objects the game made at the scene root (a corpse's gnat): their root index differs per game (2.37).
    private static readonly Dictionary<GameObject, int> SpawnedIds = [];

    private readonly Dictionary<int, Tracked> _enemies = [];
    private readonly Dictionary<HealthManager, Tracked> _byHealth = [];
    private readonly Dictionary<Fsm, FsmMode> _modes = [];
    private readonly Dictionary<int, GameObject> _objects = [];
    // Events relayed since the FSM entered this state; two alternating ones were sent every frame.
    private readonly Dictionary<Fsm, (string State, HashSet<string> Events)> _relayed = [];
    private readonly HashSet<Fsm> _localGates = [];
    private readonly HashSet<Fsm> _caughtUp = [];
    private readonly Dictionary<FsmStateAction, float> _rolls = [];
    private float _nextFrame;
    private float _lastIndex = -ReindexCooldown;
    private float _enteredAt;
    private GameObject? _hitSource;
    private int _reports;
    // The FSM whose owner state is being entered here: what its actions send is the owner's call too.
    private Fsm? _applyingTo;

    public EnemySync(Coop coop)
        : base(coop)
    {
        coop.Rooms.Entered += OnEntered;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    /// <summary>Applying a state the partner sent; patches must not echo it back.</summary>
    public static bool Applying { get; private set; }

    /// <summary>Inside a local hit on a mirrored enemy: the one death a mirror may run itself.</summary>
    public static bool LocalHit { get; set; }

    /// <summary>Inside a death the partner reported.</summary>
    public static bool RemoteDeath { get; private set; }

    /// <summary>Inside an event the mirror relayed: the partner's hero set it off.</summary>
    public static bool Relaying { get; private set; }

    /// <summary>The FSM whose check of its own body against the ground is running.</summary>
    public static Fsm? BodyCheck { get; set; }

    private bool Mirroring => !Coop.Rooms.LocalOwns;

    public override void Register(Session session)
    {
        session.On(MessageType.EnemyFrame, OnFrame);
        session.On(MessageType.FsmState, OnFsmState);
        session.On(MessageType.FsmEventRelay, OnRelayedEvent);
        session.On(MessageType.EnemyHit, OnHit);
        session.On(MessageType.EnemyDied, OnDied);
        session.On(MessageType.RoomSnapshot, OnSnapshot);
        session.On(MessageType.HazardsSpent, OnHazardsSpent);
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

    /// <summary>After all FSM updates: no velocity its FSM set moves a mirror off the owner's spot.</summary>
    public override void LateUpdate()
    {
        if (!Mirroring)
        {
            return;
        }
        foreach (Tracked enemy in _enemies.Values)
        {
            enemy.ClearVelocity();
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
        SceneManager.sceneLoaded -= OnSceneLoaded;
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
        if (fsm.Initialized)
        {
            _modes[fsm] = mode;
        }
        if (mode != FsmMode.None)
        {
            _objects[IdOf(fsm.GameObject)] = fsm.GameObject;
        }
        return mode;
    }

    /// <summary>Owner side: a synced FSM is about to switch state; the mirror follows.</summary>
    public void OnSwitch(Fsm fsm, FsmState toState)
    {
        if (Applying || !Coop.Rooms.PartnerHere || IsLocalGate(fsm))
        {
            return;
        }
        if (Coop.Rooms.LocalOwns && ModeOf(fsm) != FsmMode.None)
        {
            List<float> rolls = RollsOf(toState).Select(PreRoll).ToList();
            GameObject go = fsm.GameObject;
            Vector2? at = SyncedObjects.IsOwnerRun(go.name, fsm.Name) ? go.transform.position : null;
            SendState(IdOf(go), fsm.Name, toState.Name, rolls, at);
        }
    }

    /// <summary>A roll the owner made before entering the state (owner) or sent with it (mirror).</summary>
    public bool TakeRoll(FsmStateAction action, out float value)
    {
        if (!_rolls.TryGetValue(action, out value))
        {
            return false;
        }
        _rolls.Remove(action);
        return true;
    }

    /// <summary>Rolled before the state's actions run, so the mirror gets the value with the state.</summary>
    private float PreRoll(FsmStateAction action)
    {
        float value = action switch
        {
            RandomFloatEither either => Random.Range(0, 100) < 50 ? either.value1.Value : either.value2.Value,
            RandomFloat range => Random.Range(range.min.Value, range.max.Value),
            _ => 0f,
        };
        _rolls[action] = value;
        return value;
    }

    private static IEnumerable<FsmStateAction> RollsOf(FsmState state) =>
        (state.Actions ?? []).Where(action => action is RandomFloat or RandomFloatEither && action.Enabled);

    /// <param name="at">Owner-run objects without HP get no frames; their position rides along.</param>
    private void SendState(int id, string fsmName, string state, List<float> rolls, Vector2? at = null)
    {
        Session?.Send(MessageType.FsmState, w =>
        {
            w.Int(id).String(fsmName).String(state).Byte((byte)rolls.Count);
            rolls.ForEach(roll => w.Float(roll));
            w.Bool(at.HasValue).Float(at?.x ?? 0f).Float(at?.y ?? 0f);
        });
    }

    /// <summary>A mirrored enemy's facing is the owner's scale, sent with its frames.</summary>
    public bool MirrorsFacing(Fsm fsm)
    {
        GameObject go = fsm.GameObject;
        return Mirroring && go && go.GetComponentInParent<HealthManager>() is { } health && IsTracked(health);
    }

    /// <summary>A mirrored FSM only moves when the owner says so, even inside an applied state.</summary>
    public bool BlocksEvent(Fsm fsm) => Mirroring && ModeOf(fsm) != FsmMode.None && !IsLocalGate(fsm);

    /// <summary>Gates a wait closed in this game only (3.5); they sync again once the fight starts.</summary>
    private bool IsLocalGate(Fsm fsm)
    {
        if (PartnerWaits.Locking)
        {
            _localGates.Add(fsm);
        }
        return _localGates.Contains(fsm);
    }

    /// <summary>The owner's closed gates reach the late player (a closed owner ignores its relay).</summary>
    public void ReleaseLocalGates()
    {
        if (!Mirroring && Coop.Rooms.PartnerHere)
        {
            foreach (Fsm gate in _localGates.Where(gate => gate.GameObject && gate.ActiveState is not null))
            {
                SendState(IdOf(gate.GameObject), gate.Name, gate.ActiveStateName, []);
            }
        }
        _localGates.Clear();
    }

    /// <summary>
    /// A mirror reacted to something only it can see (its own player nearby): the owner takes the same
    /// event if it is in the same state. Once per state entry. Plain FINISHED stays the owner's call, and so
    /// does the ground under the mirror's body, which only follows positions and keeps stale velocities.
    /// </summary>
    public void RelayBlocked(Fsm fsm, FsmEvent? fsmEvent)
    {
        if (fsmEvent is null)
        {
            // SendEventByScale sends null when its event for that side is unset.
            return;
        }
        bool death = DeathEvents.Contains(fsmEvent.Name);
        bool ownersCall = fsmEvent == FsmEvent.Finished || fsm == BodyCheck || death
            || IsUnrelayed(fsm) || (_applyingTo is { } sender && IsUnrelayed(sender));
        if (fsm.ActiveState is not { } state || ownersCall || !Leads(fsm, state, fsmEvent))
        {
            return;
        }
        bool known = _relayed.TryGetValue(fsm, out (string State, HashSet<string> Events) sent);
        if (!known || sent.State != state.Name)
        {
            sent = (state.Name, []);
            _relayed[fsm] = sent;
        }
        if (!sent.Events.Add(fsmEvent.Name))
        {
            return;
        }
        int id = IdOf(fsm.GameObject);
        Session?.Send(MessageType.FsmEventRelay, w => w
            .Int(id)
            .String(fsm.Name)
            .String(state.Name)
            .String(fsmEvent.Name));
        Report($"relay out {fsm.GameObject.name} [{fsm.Name}] {state.Name} -{fsmEvent.Name}-> {Where(fsm)}");
    }

    private static bool IsUnrelayed(Fsm fsm) =>
        fsm.GameObject && SyncedObjects.IsUnrelayed(fsm.GameObject.name, fsm.Name);

    private static bool Leads(Fsm fsm, FsmState state, FsmEvent fsmEvent)
    {
        return state.Transitions.Any(t => t.FsmEvent == fsmEvent)
            || fsm.GlobalTransitions.Any(t => t.FsmEvent == fsmEvent);
    }

    /// <summary>A living enemy is out in the room; with none, nobody earns the silk a revive needs.</summary>
    public bool AnyEnemyActive => _enemies.Values.Any(enemy => enemy.IsActive);

    public bool IsMirrored(HealthManager health) => !Applying && Mirroring && IsTracked(health);

    /// <summary>By instance: an enemy's FSM may move it to another folder, which changes its path.</summary>
    private bool IsTracked(HealthManager health) => _byHealth.ContainsKey(health);

    /// <summary>The id it was tracked under, which both games agree on; its path may differ now.</summary>
    private int TrackedId(HealthManager health) =>
        _byHealth.TryGetValue(health, out Tracked? enemy) ? enemy.Id : IdOf(health.gameObject);

    public bool IsOwnedAndShared(HealthManager health)
    {
        bool owned = Coop.Rooms.LocalOwns && Coop.Rooms.PartnerHere;
        return owned && IsTracked(health);
    }

    /// <summary>A local hit on a mirrored enemy: the owner applies the same damage (D35).</summary>
    public void ReportHit(HealthManager health, int damage, HitInstance hit)
    {
        int id = TrackedId(health);
        bool killed = health.isDead;
        Report($"hit out {health.name}: damage {damage}, hp {health.hp}, killed {killed}");
        if (damage <= 0 && !killed)
        {
            return;
        }
        Session?.Send(MessageType.EnemyHit, w => w
            .Int(id)
            .Int(damage)
            .Float(hit.Direction)
            .Int((int)hit.AttackType)
            .Bool(killed));
    }

    public void ReportDeath(HealthManager health)
    {
        int id = TrackedId(health);
        Session?.Send(MessageType.EnemyDied, w => w.Int(id));
    }

    public void Track(HealthManager health)
    {
        if (!Coop.Rooms.Holds(health.gameObject.scene))
        {
            return;
        }
        if (_byHealth.ContainsKey(health))
        {
            return;
        }
        int id = IdOf(health.gameObject);
        if (!_enemies.TryGetValue(id, out Tracked? known) || known.Health != health)
        {
            Tracked enemy = new(id, health) { LastSeen = Time.unscaledTime };
            _enemies[id] = enemy;
            _byHealth[health] = enemy;
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

    /// <summary>Sent by the room's owner when the partner arrives (D12); the mirror copies it.</summary>
    public void SendSnapshot()
    {
        if (!Coop.InCoop || Coop.Rooms.Current is null)
        {
            return;
        }
        int room = Coop.Rooms.CurrentHash;
        List<Tracked> alive = _enemies.Values
            .Where(enemy => enemy.Health && !SyncedObjects.IsShortLived(enemy.Health.name))
            .ToList();
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
        SendSpentHazards(room);
    }

    /// <summary>Hazards without HP that went already (a fallen boulder) are gone for the partner.</summary>
    private void SendSpentHazards(int room)
    {
        List<PlayMakerFSM> spent = Object
            .FindObjectsByType<PlayMakerFSM>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .Where(fsm => Coop.Rooms.Holds(fsm.gameObject.scene) && Spent(fsm))
            .ToList();
        if (spent.Count == 0)
        {
            return;
        }
        Session?.Send(MessageType.HazardsSpent, w =>
        {
            w.Int(room).Int(spent.Count);
            spent.ForEach(fsm => w.Int(IdOf(fsm.gameObject)).String(fsm.FsmName));
        });
    }

    private static bool Spent(PlayMakerFSM fsm)
    {
        string? flag = SyncedObjects.SpentFlag(fsm.gameObject.name, fsm.FsmName);
        return flag is not null && fsm.FsmVariables.FindFsmBool(flag)?.Value == true;
    }

    private void OnHazardsSpent(PacketReader reader)
    {
        bool forThisRoom = reader.Int() == Coop.Rooms.CurrentHash;
        int count = reader.Int();
        for (int i = 0; i < count; i++)
        {
            PlayMakerFSM? component = FindFsm(reader.Int(), reader.String());
            if (forThisRoom && Mirroring && component is not null && !Spent(component))
            {
                component.gameObject.SetActive(false);
            }
        }
    }

    public static int IdOf(GameObject go)
    {
        for (Transform made = go.transform; made != null; made = made.parent)
        {
            if (SpawnedIds.TryGetValue(made.gameObject, out int madeId))
            {
                string below = GameRefs.PathOf(go.transform, made);
                return made == go.transform ? madeId : Fnv.Hash($"{madeId:x8}/{below}");
            }
        }
        return Fnv.Hash(go.scene.name + "/" + GameRefs.PathOf(go.transform));
    }

    /// <summary>Keeps its current id before the game moves it to the scene root (a corpse, 2.37).</summary>
    public static void KeepId(GameObject go) => SpawnedIds[go] = IdOf(go);

    /// <summary>
    /// Enemies take their id as the scene loads: a pool's sibling order changes as each game returns its
    /// worms in its own order (5.29), and the path holds sibling indexes.
    /// </summary>
    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        IEnumerable<HealthManager> enemies = scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<HealthManager>(true));
        foreach (HealthManager health in enemies)
        {
            if (!SpawnedIds.ContainsKey(health.gameObject))
            {
                KeepId(health.gameObject);
            }
        }
    }

    /// <summary>A pre-instantiated object takes its maker's id, which both games agree on (2.37).</summary>
    public void OnPreInstantiated(PreInstantiateGameObject maker)
    {
        GameObject made = maker.InstantiatedGameObject;
        if (!made)
        {
            return;
        }
        SpawnedIds[made] = Fnv.Hash($"{IdOf(maker.gameObject):x8}/made");
        foreach (HealthManager health in made.GetComponentsInChildren<HealthManager>(true))
        {
            // Tracked as it woke up inside Instantiate, under its root index.
            if (_byHealth.TryGetValue(health, out Tracked? early))
            {
                _byHealth.Remove(health);
                _enemies.Remove(early.Id);
            }
            Track(health);
        }
    }

    private void OnEntered(string scene)
    {
        _enemies.Clear();
        _byHealth.Clear();
        _modes.Clear();
        _objects.Clear();
        _relayed.Clear();
        _localGates.Clear();
        _caughtUp.Clear();
        _rolls.Clear();
        SpawnedIds.Keys.Where(made => !made).ToList().ForEach(made => SpawnedIds.Remove(made));
        _enteredAt = Time.unscaledTime;
        _reports = 0;
        foreach (HealthManager health in Resources.FindObjectsOfTypeAll<HealthManager>())
        {
            if (Coop.Rooms.Holds(health.gameObject.scene))
            {
                Track(health);
            }
        }
    }

    private FsmMode Classify(Fsm fsm)
    {
        GameObject go = fsm.GameObject;
        if (!go || !Coop.Rooms.Holds(go.scene) || go.GetComponentInParent<HeroController>(true))
        {
            return FsmMode.None;
        }
        if (fsm.IsSubFsm && HeroCutscenes.Drives(fsm, loadActions: true))
        {
            // A RunFSM template that takes the hero (Lace's Dialogue): the owner's states cannot reach it
            // (no component), so each game runs its own.
            return FsmMode.None;
        }
        if (go.GetComponentInParent<NPCControlBase>(true))
        {
            // D30: talk stays local; a synced conversation drags the partner into it.
            return FsmMode.None;
        }
        if (SyncedObjects.IsHeroWounder(fsm.Name))
        {
            // Wounds only the hero it touches here; the bite it starts goes to the owner as a relay.
            return FsmMode.None;
        }
        if (Coop.Platforms.Handles(fsm))
        {
            // Each game runs its own crumbling platform; PlatformSync starts the partner's break.
            return FsmMode.None;
        }
        bool ownerRun = SyncedObjects.IsOwnerRun(go.name, fsm.Name);
        if (go.GetComponentInParent<HealthManager>(true) || InBossArena(go.transform) || ownerRun)
        {
            return FsmMode.Authority;
        }
        bool persistent = go.GetComponent<PersistentBoolItem>() || go.GetComponent<PersistentIntItem>();
        bool named = fsm.Name == "BG Control" || SharedKeywords.Any(word => fsm.Name.Contains(word))
            || SyncedObjects.IsEitherTriggers(go.name, fsm.Name);
        bool rescue = go.name.StartsWith(FleaRescue) && ReactsToHits(fsm);
        // A synced hero cutscene (Tut_01 Weakness Scene) has both games resetting each other.
        return (persistent || named || rescue) && !HeroCutscenes.Drives(fsm) ? FsmMode.Shared : FsmMode.None;
    }

    /// <summary>Unstarted FSMs count as false: loading their actions early logs errors.</summary>
    private static bool ReactsToHits(Fsm fsm) =>
        fsm.Initialized && HeroCutscenes.ActionsOf(fsm).Any(action => action is ReceivedDamage);

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
        List<Tracked> active = _enemies.Values.Where(enemy => enemy.IsActive).ToList();
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
        List<float> rolls = [];
        int rollCount = reader.Byte();
        for (int i = 0; i < rollCount; i++)
        {
            rolls.Add(reader.Float());
        }
        bool placed = reader.Bool();
        Vector2 at = new(reader.Float(), reader.Float());
        PlayMakerFSM? component = FindFsm(id, fsmName);
        if (component is null)
        {
            return;
        }
        if (Mirroring && ModeOf(component.Fsm) != FsmMode.None && !_localGates.Contains(component.Fsm))
        {
            if (placed)
            {
                Transform body = component.transform;
                body.position = new Vector3(at.x, at.y, body.position.z);
            }
            UseRolls(component.Fsm, state, rolls);
            SetState(component, state);
        }
    }

    /// <summary>The owner's rolls replace the mirror's own when it enters the state.</summary>
    private void UseRolls(Fsm fsm, string stateName, List<float> rolls)
    {
        if (rolls.Count == 0 || fsm.GetState(stateName) is not { } state)
        {
            return;
        }
        List<FsmStateAction> actions = RollsOf(state).ToList();
        for (int i = 0; i < actions.Count && i < rolls.Count; i++)
        {
            _rolls[actions[i]] = rolls[i];
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
        if (ModeOf(component.Fsm) != FsmMode.None)
        {
            Relaying = true;
            AlertPatches.TrustPartner(component.gameObject);
            try
            {
                component.SendEvent(eventName);
            }
            finally
            {
                Relaying = false;
            }
            Report($"relay in {component.name} [{fsmName}] {state} -{eventName}-> {Where(component.Fsm)}");
        }
    }

    private void OnHit(PacketReader reader)
    {
        int id = reader.Int();
        int damage = reader.Int();
        float direction = reader.Float();
        AttackTypes attack = (AttackTypes)reader.Int();
        bool killed = reader.Bool();
        if (Mirroring)
        {
            return;
        }
        if (!_enemies.TryGetValue(id, out Tracked? enemy) || !enemy.Health)
        {
            Report($"hit in: no enemy {id:x8} here, damage {damage}");
            return;
        }
        HealthManager health = enemy.Health;
        Report($"hit in {health.name}: damage {damage}, hp {health.hp}, dead {health.isDead}, kill {killed}");
        if (health.isDead)
        {
            return;
        }
        if (killed)
        {
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
            if (forThisRoom && Mirroring && _enemies.TryGetValue(id, out Tracked? enemy) && enemy.Health)
            {
                ApplySnapshot(enemy, dead, active, hp, position, scaleX);
                states.ForEach(entry => SetState(FindFsm(entry.Object, entry.Fsm), entry.State));
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
        FsmState? left = component.Fsm.ActiveState;
        Applying = true;
        _applyingTo = component.Fsm;
        try
        {
            foreach (string step in StartChain(component.Fsm, state))
            {
                EnterState(component, step);
            }
            EnterState(component, state);
        }
        finally
        {
            Applying = false;
            _applyingTo = null;
        }
        if (left is not null && left.Name != state && (left.Actions ?? []).OfType<RunFSM>().Any())
        {
            ReleaseTemplateBlocker(component.gameObject);
        }
    }

    /// <summary>
    /// The owner's template ran to its end, this game's copy was stopped midway: its input blocker on the
    /// local hero would stay for good (Lace's Dialogue, marks 26/27).
    /// </summary>
    private void ReleaseTemplateBlocker(GameObject owner)
    {
        if (GameRefs.Hero is { } hero && Patches.HeroControlLogPatches.Blocks(hero, owner))
        {
            hero.RemoveInputBlocker(owner);
            Coop.Log.LogInfo($"released input blocker of {owner.name}");
        }
    }

    /// <summary>
    /// A mirror still in its start state never ran the owner's setup (Lace's Init sets Self): the FINISHED
    /// states between it and the first applied one, once per room.
    /// </summary>
    private IEnumerable<string> StartChain(Fsm fsm, string target)
    {
        List<string> chain = [];
        if (!_caughtUp.Add(fsm) || fsm.ActiveState is not { } at || at.Name != fsm.StartState)
        {
            return chain;
        }
        for (int i = 0; i < StartChainDepth; i++)
        {
            string? next = at.Transitions.FirstOrDefault(t => t.FsmEvent == FsmEvent.Finished)?.ToState;
            if (next is null || next == target || fsm.GetState(next) is not { } state)
            {
                break;
            }
            chain.Add(next);
            at = state;
        }
        return chain;
    }

    /// <summary>A throwing action leaves its FSM on PlayMaker's execution stack; this takes it off.</summary>
    private static void EnterState(PlayMakerFSM component, string state)
    {
        Fsm? before = FsmExecutionStack.ExecutingFsm;
        try
        {
            component.SetState(state);
        }
        catch
        {
            while (FsmExecutionStack.ExecutingFsm is { } leaked && leaked != before)
            {
                FsmExecutionStack.PopFsm();
            }
            throw;
        }
    }

    /// <summary>A death the partner already had, not reported back; loot drops here too.</summary>
    private static void DieQuietly(HealthManager health, float? direction, AttackTypes attack)
    {
        RemoteDeath = true;
        try
        {
            health.Die(direction, attack, NailElements.None, null, true, 1f, false, false);
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
        if (Time.unscaledTime - _lastIndex < ReindexCooldown || Coop.Rooms.Current is null)
        {
            return;
        }
        _lastIndex = Time.unscaledTime;
        foreach (PlayMakerFSM fsm in Resources.FindObjectsOfTypeAll<PlayMakerFSM>())
        {
            if (Coop.Rooms.Holds(fsm.gameObject.scene))
            {
                _objects[IdOf(fsm.gameObject)] = fsm.gameObject;
            }
        }
    }

    /// <summary>Diagnostics for enemies that ignore one player: a bounded number of lines per room.</summary>
    private void Report(string text)
    {
        if (_reports < MaxReports)
        {
            _reports++;
            Coop.Log.LogInfo(text);
        }
    }

    private string Where(Fsm fsm)
    {
        Vector2 enemy = fsm.GameObject.transform.position;
        Vector2? hero = GameRefs.Hero is { } local ? local.transform.position : null;
        Vector2? partner = Coop.Heroes.PartnerTransform is { } drawn ? drawn.position : null;
        return $"enemy {enemy}, hero {hero?.ToString() ?? "-"}, partner {partner?.ToString() ?? "-"}";
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

        public bool IsActive => Health && !Health.isDead && Health.gameObject.activeInHierarchy;

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

        public void ClearVelocity()
        {
            if (_ownBodyType is not null && Health && Health.TryGetComponent(out Rigidbody2D body))
            {
                body.linearVelocity = Vector2.zero;
            }
        }
    }
}
