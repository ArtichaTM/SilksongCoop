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

        BossHp = file.Bind("Scaling", "BossHpMultiplier", 1.5f, "Boss HP multiplier while a friend is in");
        EnemyHp = file.Bind("Scaling", "EnemyHpMultiplier", 1f, "Other enemies' HP multiplier");
        BossHpThreshold = file.Bind(
            "Scaling",
            "BossHpThreshold",
            400,
            "Base HP from which an enemy without a boss title counts as a boss; 0 disables"
        );

        ReviverHealed = file.Bind("Revive", "ReviverHealed", false, "Reviving Bind also heals the reviver");
        CocoonMoves = file.Bind("Revive", "DownedMovesCocoon", false, "A downed player can roll the cocoon");
        CocoonSpeed = file.Bind("Revive", "CocoonSpeed", 2f, "How fast a downed player rolls the cocoon");
        CocoonRise = file.Bind("Revive", "CocoonRiseLimit", 4f, "How high above its spawn the cocoon rises");
        CameraKey = file.Bind("Revive", "CameraKey", KeyCode.V, "While down: watch cocoon or partner");

        PauseFreezesBoth = file.Bind("Pause", "PauseFreezesBoth", false, "Pausing freezes both games (D29)");
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

    public ConfigEntry<float> BossHp { get; }

    public ConfigEntry<float> EnemyHp { get; }

    public ConfigEntry<int> BossHpThreshold { get; }

    public ConfigEntry<bool> ReviverHealed { get; }

    public ConfigEntry<bool> CocoonMoves { get; }

    public ConfigEntry<float> CocoonSpeed { get; }

    public ConfigEntry<float> CocoonRise { get; }

    public ConfigEntry<KeyCode> CameraKey { get; }

    public ConfigEntry<bool> PauseFreezesBoth { get; }
}
