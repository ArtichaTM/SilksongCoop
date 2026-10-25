using System;
using BepInEx.Configuration;
using UnityEngine;

namespace SilksongCoop;

/// <summary>Every setting lives in the BepInEx config file (D24, D27); there is no in-game UI.</summary>
internal sealed class CoopConfig
{
    public CoopConfig(ConfigFile file)
    {
        PlayerName = file.Bind("General", "PlayerName", Environment.UserName, "Name shown above your Hornet");
        Password = file.Bind("General", "Password", "", "Shared password; host and friend must match (D34)");

        HostKey = file.Bind("Host", "HostKey", KeyCode.F8, "Start or stop hosting while in game");
        HostOnStart = file.Bind("Host", "HostOnStart", false, "Host automatically once a save is loaded");
        HostPort = file.Bind("Host", "Port", 26950, "UDP port the host listens on");

        HostAddress = file.Bind("Join", "Address", "", "Host's domain name or IP");
        JoinPort = file.Bind("Join", "Port", 26950, "Host's UDP port");
        JoinKey = file.Bind("Join", "JoinKey", KeyCode.F7, "Join the host from the main menu");

        Color lightBlue = new(0.55f, 0.85f, 1f, 1f);
        Tint = file.Bind("Partner", "Tint", lightBlue, "Colour of the partner's Hornet (D23)");
        ShowPointer = file.Bind("Partner", "ShowPointer", true, "Arrow towards an off-screen partner");

        BossHp = file.Bind("Scaling", "BossHpMultiplier", 1.5f, "Boss HP multiplier while a friend is in");
        EnemyHp = file.Bind("Scaling", "EnemyHpMultiplier", 1f, "Other enemies' HP multiplier");
        BossHpThreshold = file.Bind(
            "Scaling",
            "BossHpThreshold",
            400,
            "Base HP from which an enemy without a boss title counts as a boss; 0 disables"
        );

        ReviverHealed = file.Bind("Revive", "ReviverHealed", false, "Reviving Bind also heals the reviver");
        // Deprecated 3.6 b: unused since 2026-10-25 and not maintained; renamed so saved "true" values lapse.
        const string Unused = "Unused since 2026-10-25, not maintained: ";
        const string MovesKey = "DownedMovesCocoonUnused";
        CocoonMoves = file.Bind("Revive", MovesKey, false, Unused + "a downed player rolls the cocoon");
        CocoonSpeed = file.Bind("Revive", "CocoonSpeed", 2f, Unused + "how fast the cocoon rolls");
        CocoonRise = file.Bind("Revive", "CocoonRiseLimit", 4f, Unused + "how high the cocoon rises");
        CameraKey = file.Bind("Revive", "CameraKey", KeyCode.V, "While down: watch cocoon or partner");
        ForceReviveFullHealth = file.Bind(
            "Revive",
            "ForceReviveFullHealth",
            true,
            "Getting up without a Bind (no enemy left, summoned to the partner's fight) restores "
                + "full health; off: 1 health"
        );
        SelfReviveDelay = file.Bind(
            "Revive",
            "SelfReviveDelay",
            10f,
            "Seconds after which a downed player gets up alone once no enemy is left in the room; 0 disables"
        );

        PauseFreezesBoth = file.Bind("Pause", "PauseFreezesBoth", false, "Pausing freezes both games (D29)");

        SyncPlatforms = file.Bind(
            "World",
            "SyncCrumblingPlatforms",
            true,
            "A platform that crumbles and reforms breaks in your game too when the partner touches it"
        );

        DevEnabled = file.Bind("Dev", "Enabled", false, "Dev tools; a save they ran on is deleted on exit");
        DevInvulnerable = file.Bind("Dev", "Invulnerable", false, "No hero damage in dev mode");
        DevKey = file.Bind("Dev", "DevModeKey", KeyCode.F1, "Dev mode on or off: one-hit kills, bug report");
        BringKey = file.Bind("Dev", "BringPartnerKey", KeyCode.F2, "Bring the partner to where you stand");
        FlyKey = file.Bind("Dev", "FlyKey", KeyCode.F3, "Toggle free fly in dev mode");
        GiveAllKey = file.Bind("Dev", "GiveAllKey", KeyCode.F4, "With Shift: give all skills, tools, crests");
        const string TeleportHelp = "Teleport menu: the scenes and objects the sync verdicts name";
        TeleportKey = file.Bind("Dev", "TeleportKey", KeyCode.F5, TeleportHelp);
        FlySpeed = file.Bind("Dev", "FlySpeed", 20f, "Free fly speed; Dash held flies 2.5 times faster");
    }

    public ConfigEntry<string> PlayerName { get; }

    public ConfigEntry<string> Password { get; }

    public ConfigEntry<KeyCode> HostKey { get; }

    public ConfigEntry<bool> HostOnStart { get; }

    public ConfigEntry<int> HostPort { get; }

    public ConfigEntry<string> HostAddress { get; }

    public ConfigEntry<int> JoinPort { get; }

    public ConfigEntry<KeyCode> JoinKey { get; }

    public ConfigEntry<Color> Tint { get; }

    public ConfigEntry<bool> ShowPointer { get; }

    public ConfigEntry<float> BossHp { get; }

    public ConfigEntry<float> EnemyHp { get; }

    public ConfigEntry<int> BossHpThreshold { get; }

    public ConfigEntry<bool> ReviverHealed { get; }

    public ConfigEntry<bool> CocoonMoves { get; }

    public ConfigEntry<float> CocoonSpeed { get; }

    public ConfigEntry<float> CocoonRise { get; }

    public ConfigEntry<KeyCode> CameraKey { get; }

    public ConfigEntry<bool> ForceReviveFullHealth { get; }

    public ConfigEntry<float> SelfReviveDelay { get; }

    public ConfigEntry<bool> PauseFreezesBoth { get; }

    public ConfigEntry<bool> SyncPlatforms { get; }

    public ConfigEntry<bool> DevEnabled { get; }

    public ConfigEntry<bool> DevInvulnerable { get; }

    public ConfigEntry<KeyCode> DevKey { get; }

    public ConfigEntry<KeyCode> BringKey { get; }

    public ConfigEntry<KeyCode> FlyKey { get; }

    public ConfigEntry<KeyCode> GiveAllKey { get; }

    public ConfigEntry<KeyCode> TeleportKey { get; }

    public ConfigEntry<float> FlySpeed { get; }
}
