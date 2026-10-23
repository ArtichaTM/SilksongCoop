namespace SilksongCoop.Net;

internal enum MessageType : byte
{
    // Unreliable: the next one replaces a lost one.
    Heartbeat,
    HeroFrame,
    HeroParts,
    EnemyFrame,

    // Reliable and ordered.
    Hello,
    Welcome,
    Reject,
    Goodbye,
    SaveChunk,
    RoomInfo,
    RoomSnapshot,
    FsmState,
    EnemyHit,
    EnemyDied,
    BattleStart,
    Pull,
    Teleport,
    Downed,
    Revive,
    BothDown,
    FieldChange,
    SceneDataChange,
    Pause,
    HeroSound,
    ClipSound,
    DialogueSkip,
    FsmEventRelay,
    CocoonBroken,
    ToolThrown,
    DevMode,
    InventoryCheck,
    PickupTaken,
    GotUp,
    ShopBought,
    PlatformBreak,
    BreakablesBroken,
}

/// <summary>First byte after the magic: how the rest of the datagram is framed.</summary>
internal enum Framing : byte
{
    Unreliable,
    Reliable,
    Ack,
}
