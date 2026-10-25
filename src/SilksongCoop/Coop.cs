using System;
using System.Collections.Generic;
using System.Net.Sockets;
using BepInEx;
using BepInEx.Logging;
using SilksongCoop.Game;
using SilksongCoop.Net;
using SilksongCoop.Dev;
using UnityEngine;

namespace SilksongCoop;

/// <summary>
/// The hub: owns the session and the feature modules, and is what Harmony patches reach through
/// <see cref="Current"/>. Null outside the plugin's lifetime.
/// </summary>
internal sealed class Coop : IDisposable
{
    private readonly List<Module> _modules;
    private readonly HashSet<MessageType> _oversized = [];
    private bool _hostedOnStart;

    public Coop(BaseUnityPlugin plugin, CoopConfig config, ManualLogSource log)
    {
        Plugin = plugin;
        Config = config;
        Log = log;
        Rooms = new Rooms(this);
        Heroes = new HeroSync(this);
        Enemies = new EnemySync(this);
        Downed = new DownedState(this);
        Pulls = new Pulls(this);
        World = new WorldSync(this);
        Joining = new JoinFlow(this);
        Pause = new PauseSync(this);
        HeroAudio = new HeroAudioSync(this);
        ClipAudio = new ClipAudioSync(this);
        Dialogue = new DialogueSync(this);
        Preload = new AssetPreload(this);
        Tools = new ToolReplicas(this);
        Pickups = new PickupSync(this);
        Shops = new ShopSync(this);
        Spending = new SpendLog(this);
        Tolls = new TollSync(this);
        Platforms = new PlatformSync(this);
        Breakables = new BreakableSync(this);
        Memories = new MemorySync(this);
        HitResponses = new HitResponseSync(this);
        Replays = new ReplaySync(this);
        Persistent = new PersistentSync(this);
        Levers = new LeverSync(this);
        Plates = new PlateSync(this);
        BellBeast = new BellBeastSync(this);
        Waits = new PartnerWaits(this);
        Pointer = new PartnerPointer(this);
        Dev = new DevTools(this);
        _modules =
        [
            Joining, Rooms, World, Preload, Heroes, Tools, HeroAudio, ClipAudio, Dialogue, Enemies, Platforms,
            Breakables, Memories, HitResponses, Replays, Persistent, Levers, Plates, Waits, BellBeast, Downed,
            Pulls, Pause, Pickups, Shops, Spending, Tolls, Pointer, Dev,
        ];
        Current = this;
    }

    public static Coop? Current { get; private set; }

    public BaseUnityPlugin Plugin { get; }

    public CoopConfig Config { get; }

    public ManualLogSource Log { get; }

    public Partner Partner { get; } = new();

    public Hints Hints { get; } = new();

    public Rooms Rooms { get; }

    public HeroSync Heroes { get; }

    public EnemySync Enemies { get; }

    public DownedState Downed { get; }

    public Pulls Pulls { get; }

    public WorldSync World { get; }

    public JoinFlow Joining { get; }

    public PauseSync Pause { get; }

    public HeroAudioSync HeroAudio { get; }

    public ClipAudioSync ClipAudio { get; }

    public DialogueSync Dialogue { get; }

    public AssetPreload Preload { get; }

    public ToolReplicas Tools { get; }

    public PickupSync Pickups { get; }

    public ShopSync Shops { get; }

    public SpendLog Spending { get; }

    public TollSync Tolls { get; }

    public PlatformSync Platforms { get; }

    public BreakableSync Breakables { get; }

    public MemorySync Memories { get; }

    public HitResponseSync HitResponses { get; }

    public ReplaySync Replays { get; }

    public PersistentSync Persistent { get; }

    public LeverSync Levers { get; }

    public PlateSync Plates { get; }

    public BellBeastSync BellBeast { get; }

    public PartnerWaits Waits { get; }

    public PartnerPointer Pointer { get; }

    public DevTools Dev { get; }

    public Session? Session { get; private set; }

    /// <summary>Both games are connected and past the handshake.</summary>
    public bool InCoop => Session is { Connected: true };

    public bool IsHost => Session is { IsHost: true };

    /// <summary>This game plays the host's save (D6): from the join request until back in the menu.</summary>
    public bool IsFriend => Session is { IsHost: false } || Joining.PlayingHostSave;

    private Credentials Credentials => new(Handshake.ProtocolVersion, GameBuild.Fingerprint, Password);

    private string Password => Config.Password.Value;

    /// <summary>This game's quit-to-menu coroutine is running; set until the menu scene is up.</summary>
    public bool QuittingToMenu { get; private set; }

    public void Update()
    {
        HandleKeys();
        if (GameRefs.InMenu)
        {
            QuittingToMenu = false;
            if (IsHost)
            {
                EndSession("the host quit to the menu");
            }
            _hostedOnStart = false;
        }
        Session?.Tick();
        foreach (Module module in _modules)
        {
            module.Update();
        }
        Hints.Status = StatusLine();
    }

    public void LateUpdate()
    {
        foreach (Module module in _modules)
        {
            module.LateUpdate();
        }
    }

    public void OnGUI()
    {
        Hints.Draw();
        Dev.Draw();
    }

    public void StartHosting()
    {
        try
        {
            Session = Session.Host(Config.HostPort.Value, Credentials, Config.PlayerName.Value);
            Wire(Session);
            Hints.Show($"Hosting on UDP port {Config.HostPort.Value}");
        }
        catch (SocketException exception)
        {
            Hints.Show($"Cannot host: {exception.Message}", 6f);
        }
    }

    public void JoinHost()
    {
        string address = Config.HostAddress.Value.Trim();
        if (address.Length == 0)
        {
            Hints.Show("Set [Join] Address in the config first", 6f);
            return;
        }
        try
        {
            Session = Session.Join(address, Config.JoinPort.Value, Credentials, Config.PlayerName.Value);
            Wire(Session);
            Hints.Show($"Joining {address}...");
        }
        catch (Exception exception) when (exception is SocketException or System.IO.IOException)
        {
            Hints.Show($"Cannot join: {exception.Message}", 6f);
        }
    }

    /// <summary>D31: quit was pressed; the partner learns now, so both quits never overlap.</summary>
    public void OnQuitToMenu()
    {
        QuittingToMenu = true;
        EndSession(IsHost ? "the host quit to the menu" : "partner quit to the menu");
    }

    /// <summary>Closes the session for good; the host stops listening, the friend stops joining.</summary>
    public void EndSession(string reason)
    {
        if (Session is null)
        {
            return;
        }
        bool wasInCoop = InCoop;
        Session.Dispose();
        Session = null;
        if (wasInCoop)
        {
            PartnerGone();
        }
        Log.LogInfo($"session ended: {reason}");
    }

    public void Dispose()
    {
        if (IsFriend)
        {
            // The reloaded plugin won't know this is the host's save and would let the game save it.
            Joining.HostGone("coop plugin reloaded");
        }
        EndSession("plugin unloaded");
        foreach (Module module in _modules)
        {
            try
            {
                module.Dispose();
            }
            catch (Exception exception)
            {
                // C2: one module's failure must not leave the others' objects behind.
                Log.LogError($"{module.GetType().Name} cleanup failed: {exception}");
            }
        }
        Current = null;
    }

    private void Wire(Session session)
    {
        session.Joined += OnJoined;
        session.Left += OnLeft;
        session.Oversized += (type, size) =>
        {
            if (_oversized.Add(type))
            {
                Log.LogWarning($"{type} sent as one {size}-byte datagram; routers may drop it");
            }
        };
        session.HandlerFailed += (type, exception) => Log.LogError($"{type} handler failed: {exception}");
        foreach (Module module in _modules)
        {
            module.Register(session);
        }
    }

    private void OnJoined()
    {
        Partner.Name = Session!.PartnerName;
        Hints.Show($"{Partner.Name} connected");
        Log.LogInfo($"partner {Partner.Name} connected");
        foreach (Module module in _modules)
        {
            module.OnJoined();
        }
    }

    private void OnLeft(string reason)
    {
        Log.LogInfo($"partner left: {reason}");
        PartnerGone();
        if (Session is { IsHost: false })
        {
            // D31: the friend's game cannot run the host's world alone; HostGone shows the hint.
            Session.Dispose();
            Session = null;
            Joining.HostGone(reason);
        }
        else
        {
            Hints.Show($"Partner left: {reason}", 6f);
        }
    }

    private void PartnerGone()
    {
        foreach (Module module in _modules)
        {
            try
            {
                module.OnLeft();
            }
            catch (System.Exception exception)
            {
                // One module's cleanup must not stop the rest, or the friend never returns to the menu.
                Log.LogError($"{module.GetType().Name} partner cleanup failed: {exception}");
            }
        }
        Partner.Reset();
    }

    private void HandleKeys()
    {
        bool inGame = GameRefs.GameplayScene is not null && !QuittingToMenu;
        if (inGame && !IsFriend && Input.GetKeyDown(Config.HostKey.Value))
        {
            if (Session is null)
            {
                StartHosting();
            }
            else
            {
                EndSession("host stopped hosting");
                Hints.Show("Stopped hosting");
            }
        }
        if (inGame && !IsFriend && Session is null && Config.HostOnStart.Value && !_hostedOnStart)
        {
            _hostedOnStart = true;
            StartHosting();
        }
        if (GameRefs.InMenu && Session is null && Input.GetKeyDown(Config.JoinKey.Value))
        {
            Log.LogInfo($"join key pressed, joining {Config.HostAddress.Value}:{Config.JoinPort.Value}");
            JoinHost();
        }
    }

    private string StatusLine()
    {
        if (Session is null)
        {
            string target = $"{Config.HostAddress.Value}:{Config.JoinPort.Value}";
            return GameRefs.InMenu ? $"Coop: press {Config.JoinKey.Value} to join {target}" : "";
        }
        if (InCoop)
        {
            return $"Coop: {(IsHost ? "hosting" : "joined")}, partner {Partner.Name}";
        }
        return IsHost ? "Coop: hosting, waiting for partner" : "Coop: connecting...";
    }
}
