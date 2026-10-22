using System.Collections.Generic;

namespace SilksongCoop.Rules;

internal enum FieldScope
{
    /// <summary>One value in the host's save, whoever changes it (D21, D36a, D36b).</summary>
    Shared,

    /// <summary>
    /// The host's game writes it; the friend's copy follows the host (D42). A change the friend makes in the
    /// host's room counts too: a scripted sequence there (a memory's end) may run in the friend's game only.
    /// </summary>
    HostOwned,

    /// <summary>Each game keeps its own: HP, silk, where the player stands.</summary>
    PerPlayer,

    /// <summary>Crest, tools and skills equipped (D20); per player, stored on the friend's PC.</summary>
    Loadout,
}

/// <summary>Which <c>PlayerData</c> fields the two games share and how.</summary>
internal static class FieldRules
{
    private static readonly HashSet<string> LoadoutFields =
    [
        "CurrentCrestID", "PreviousCrestID", "ToolEquips", "ExtraToolEquips",
    ];

    private static readonly HashSet<string> HostOwnedFields =
    [
        "respawnScene", "respawnMarkerName", "respawnType",
    ];

    private static readonly HashSet<string> PerPlayerFields =
    [
        "LastSetFieldName", "version", "RevisionBreak", "date", "profileID", "playTime",
        "health", "healthBlue", "prevHealth", "silk", "atBench", "isInvincible", "infiniteAirJump",
        "mapZone", "extraRestZone", "hazardRespawnFacing", "PreMemoryState", "HasStoredMemoryState",
        "currentArea", "currentInvPane", "mapKeyPref", "environmentType", "previousDarkness",
        "dreamReturnScene", "bossReturnEntryGate",
    ];

    /// <summary>What the players carry; the host checks the friend's copy and corrects drift.</summary>
    private static readonly HashSet<string> InventoryFields =
    [
        "geo", "ShellShards", "Tools", "ToolLiquids", "Collectables", "Relics", "MementosDeposited",
        "MateriumCollected", "QuestCompletionData",
    ];

    public static IReadOnlyCollection<string> Loadout => LoadoutFields;

    public static IReadOnlyCollection<string> Inventory => InventoryFields;

    public static FieldScope Of(string field)
    {
        if (LoadoutFields.Contains(field))
        {
            return FieldScope.Loadout;
        }
        if (HostOwnedFields.Contains(field))
        {
            return FieldScope.HostOwned;
        }
        return PerPlayerFields.Contains(field) ? FieldScope.PerPlayer : FieldScope.Shared;
    }

    public static bool HostSends(FieldScope scope) => scope is FieldScope.Shared or FieldScope.HostOwned;

    public static bool FriendSends(FieldScope scope, bool sameRoom) =>
        scope == FieldScope.Shared || (sameRoom && scope == FieldScope.HostOwned);
}
