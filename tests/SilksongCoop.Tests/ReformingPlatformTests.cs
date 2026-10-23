using SilksongCoop.Rules;

namespace SilksongCoop.Tests;

public class ReformingPlatformTests
{
    /// <summary>Memory_Silk_Heart_BellBeast memory_ground_plat [Control].</summary>
    private static readonly Dictionary<string, IReadOnlyList<FsmLink>> MemoryPlat = new()
    {
        ["Init"] = [new("FINISHED", "Idle")],
        ["Idle"] = [new("TILT PLAT LANDED", "Break Antic")],
        ["Break Antic"] = [new("FINISHED", "Break")],
        ["Break"] = [new("FINISHED", "Reform")],
        ["Reform"] = [new("FINISHED", "Reactivate")],
        ["Reactivate"] = [new("FINISHED", "Idle")],
    };

    /// <summary>Bone_06 bone_plat_01_crumble [bone_crumble_plat], the touch check inside the FSM.</summary>
    private static readonly Dictionary<string, IReadOnlyList<FsmLink>> CrumblePlat = new()
    {
        ["Init"] = [new("FINISHED", "Idle")],
        ["Idle"] =
        [
            new("COLLIDE", "Check Collision"), new("BONK", "Break"), new("CRUMBLE", "Crumble Antic"),
            new("DAMAGED", "Crumble Antic"),
        ],
        ["Check Collision"] =
        [
            new("CRUMBLE", "Crumble Antic"), new("CANCEL", "Idle"), new("H CRUMBLE", "Check Collision 2"),
        ],
        ["Check Collision 2"] = [new("CRUMBLE", "Crumble Antic"), new("CANCEL", "Idle")],
        ["Crumble Antic"] =
        [
            new("FINISHED", "Break"), new("BONK", "Break"), new("HAZARD RESPAWNED", "Hazard Respawn Reset"),
        ],
        ["Break"] = [new("FINISHED", "Core Antic"), new("HAZARD RESPAWNED", "Hazard Respawn Reset")],
        ["Core Antic"] = [new("FINISHED", "Rebuild")],
        ["Rebuild"] = [new("FINISHED", "Shake?")],
        ["Shake?"] = [new("FINISHED", "Reform")],
        ["Reform"] = [new("FINISHED", "Idle")],
        ["Hazard Respawn Reset"] = [new("FINISHED", "Idle")],
    };

    [Theory]
    [InlineData("Idle", "Break Antic", true)]
    [InlineData("Break Antic", "Break", false)]
    [InlineData("Reactivate", "Idle", false)]
    public void MemoryPlatformBreaksFromIdle(string from, string to, bool starts) =>
        Assert.Equal(starts, ReformingPlatform.Of(MemoryPlat)!.Starts(from, to));

    [Theory]
    [InlineData("Idle", "Check Collision", false)]
    [InlineData("Check Collision", "Crumble Antic", true)]
    [InlineData("Idle", "Break", true)]
    [InlineData("Crumble Antic", "Break", false)]
    [InlineData("Reform", "Idle", false)]
    public void CrumblePlatformBreaksAfterItsOwnTouchCheck(string from, string to, bool starts) =>
        Assert.Equal(starts, ReformingPlatform.Of(CrumblePlat)!.Starts(from, to));

    [Fact]
    public void ABreakingPlatformTakesNoSecondStart()
    {
        ReformingPlatform platform = ReformingPlatform.Of(CrumblePlat)!;
        Assert.True(platform.Rests("Check Collision"));
        Assert.False(platform.Rests("Init"));
        Assert.False(platform.Rests("Core Antic"));
        Assert.False(platform.IsTrigger("Core Antic"));
    }

    [Fact]
    public void ABreakThatNeverReformsIsNoPlatform()
    {
        Dictionary<string, IReadOnlyList<FsmLink>> bridge = new()
        {
            ["Idle"] = [new("HIT", "Break")],
            ["Break"] = [],
        };
        Assert.Null(ReformingPlatform.Of(bridge));
    }
}
