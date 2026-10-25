using HarmonyLib;
using SilksongCoop.Game;
using SilksongCoop.Net;
using UnityEngine;

namespace SilksongCoop.Dev;

/// <summary>
/// Testing tools for running through the game in coop: one-hit kills in both games, free fly, give
/// everything, bring the partner over, teleport to audited objects, and a bug report. Off by default;
/// when off it binds no keys and patches nothing. It works on any save and deletes nothing; F4 changes
/// the save for good.
/// </summary>
internal sealed class DevTools : Module
{
    private static readonly string[] Abilities =
    [
        "hasDash", "hasWalljump", "hasDoubleJump", "hasBrolly", "hasSuperJump", "hasHarpoonDash",
        "hasChargeSlash", "hasNeedleThrow", "hasThreadSphere", "hasParry", "hasSilkCharge", "hasSilkBomb",
        "hasSilkBossNeedle", "hasSilkSpecial", "hasNeedolin",
    ];

    private readonly bool _enabled;
    private readonly FreeFly _fly;
    private readonly TeleportMenu? _teleports;
    private readonly Harmony? _harmony;
    private bool _on;
    private bool _hostOn;
    private DevReport? _report;
    private DevChecks? _checks;

    public DevTools(Coop coop)
        : base(coop)
    {
        _enabled = coop.Config.DevEnabled.Value;
        _fly = new FreeFly(coop.Config);
        if (!_enabled)
        {
            return;
        }
        _teleports = new TeleportMenu(coop, _fly);
        _harmony = new Harmony(Plugin.Guid + ".dev");
        FreeFly.Patch(_harmony);
    }

    /// <summary>This game is in dev mode, its own or the host's.</summary>
    private bool Active => _on || _hostOn;

    public override void Register(Session session) => session.On(MessageType.DevMode, OnDevMode);

    public override void Update()
    {
        if (!_enabled)
        {
            return;
        }
        if (GameRefs.InMenu)
        {
            SetOn(false, "back in the menu");
            _hostOn = false;
        }
        HandleKeys();
        UpdateReport();
        ApplyCheats();
        _teleports!.Update();
    }

    public void Draw() => _teleports?.Draw();

    public override void OnJoined()
    {
        if (_on)
        {
            SendMode(true);
        }
    }

    public override void OnLeft()
    {
        _hostOn = false;
        if (!_on)
        {
            _fly.Stop();
        }
    }

    public override void Dispose()
    {
        if (!_enabled)
        {
            return;
        }
        _teleports!.Close();
        SetOn(false, "plugin unloaded");
        _hostOn = false;
        UpdateReport();
        CheatManager.NailDamage = CheatManager.NailDamageStates.Normal;
        CheatManager.Invincibility = CheatManager.InvincibilityStates.Off;
        _harmony?.UnpatchSelf();
    }

    private void HandleKeys()
    {
        CoopConfig config = Coop.Config;
        if (Input.GetKeyDown(config.DevKey.Value))
        {
            ToggleDevMode();
        }
        if (!Active)
        {
            _teleports!.Close();
            return;
        }
        if (Input.GetKeyDown(config.TeleportKey.Value))
        {
            _teleports!.Toggle();
        }
        if (Coop.InCoop && Input.GetKeyDown(config.BringKey.Value))
        {
            Coop.Pulls.BringPartner();
            _report?.Line("brought the partner here");
            Coop.Hints.Show($"Bringing {Coop.Partner.Name} here");
        }
        if (Input.GetKeyDown(config.FlyKey.Value))
        {
            bool toggled = _fly.Toggle();
            Coop.Hints.Show(toggled ? $"Free fly {(_fly.Flying ? "on" : "off")}" : "Cannot fly now");
        }
        bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        if (shift && GameRefs.HeroReady && Input.GetKeyDown(config.GiveAllKey.Value))
        {
            GiveEverything();
            _report?.Line("gave everything");
            Coop.Hints.Show("Gave all abilities, tools and crests");
        }
    }

    private void ToggleDevMode()
    {
        if (_on)
        {
            SetOn(false, "turned off");
            Coop.Hints.Show("Dev mode off");
            return;
        }
        if (CannotStart() is { } why)
        {
            Coop.Hints.Show(why, 5f);
            return;
        }
        SetOn(true, "turned on");
        const string Keys = "F2 brings partner, F3 fly, Shift+F4 give all, F5 teleport";
        Coop.Hints.Show($"Dev mode on: one-hit kills, {Keys}", 5f);
    }

    /// <summary>Null when dev mode may start here.</summary>
    private string? CannotStart()
    {
        if (Coop.IsFriend)
        {
            return "The host turns dev mode on";
        }
        return GameRefs.GameplayScene is null ? "Load a save first" : null;
    }

    private void SetOn(bool on, string reason)
    {
        if (_on == on)
        {
            return;
        }
        _on = on;
        Coop.Log.LogInfo($"dev mode {reason}");
        SendMode(on);
        if (!on)
        {
            _fly.Stop();
        }
    }

    private void SendMode(bool on)
    {
        if (Coop.IsHost && Coop.InCoop)
        {
            Session?.Send(MessageType.DevMode, w => w.Bool(on));
        }
    }

    /// <summary>Friend side: the host's dev mode applies here too, one-hit kills included.</summary>
    private void OnDevMode(PacketReader reader)
    {
        bool on = reader.Bool();
        if (_enabled)
        {
            _hostOn = on;
            if (!Active)
            {
                _fly.Stop();
            }
        }
    }

    /// <summary>The report is open exactly while dev mode is.</summary>
    private void UpdateReport()
    {
        if (Active && _report is null)
        {
            _report = new DevReport();
            _checks = new DevChecks(Coop, _report);
            _report.Line("dev mode on");
            Coop.Log.LogInfo($"dev report {_report.Path}");
        }
        else if (!Active && _report is not null)
        {
            _report.Line("dev mode off");
            _checks!.Dispose();
            _checks = null;
            _report.Dispose();
            _report = null;
        }
        _checks?.Update();
    }

    private void ApplyCheats()
    {
        bool invulnerable = Active && (Coop.Config.DevInvulnerable.Value || _fly.Flying);
        CheatManager.NailDamage = Active
            ? CheatManager.NailDamageStates.InstaKill
            : CheatManager.NailDamageStates.Normal;
        CheatManager.Invincibility = invulnerable
            ? CheatManager.InvincibilityStates.FullInvincible
            : CheatManager.InvincibilityStates.Off;
    }

    /// <summary>Shared save fields: on the friend's side world sync passes them to the host.</summary>
    private static void GiveEverything()
    {
        foreach (string ability in Abilities)
        {
            PlayerData.instance.SetBool(ability, true);
        }
        ToolItemManager.UnlockAllTools();
        ToolItemManager.UnlockAllCrests();
    }
}
