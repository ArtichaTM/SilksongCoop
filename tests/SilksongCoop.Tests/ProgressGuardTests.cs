using Newtonsoft.Json.Linq;
using SilksongCoop.Rules;

namespace SilksongCoop.Tests;

public class ProgressGuardTests
{
    [Fact]
    public void LearnedAbilitiesStayLearned()
    {
        Assert.False(ProgressGuard.Allows("hasDash", new JValue(true), new JValue(false)));
        Assert.True(ProgressGuard.Allows("hasDash", new JValue(false), new JValue(true)));
        Assert.False(ProgressGuard.Allows("UnlockedFastTravel", new JValue(true), new JValue(false)));
        Assert.False(ProgressGuard.Allows("HasMossGrottoMap", new JValue(true), new JValue(false)));
    }

    [Fact]
    public void UpgradesNeverShrink()
    {
        Assert.False(ProgressGuard.Allows("maxHealth", new JValue(9), new JValue(5)));
        Assert.False(ProgressGuard.Allows("nailUpgrades", new JValue(4), new JValue(0)));
        Assert.True(ProgressGuard.Allows("maxHealth", new JValue(9), new JValue(10)));
    }

    [Fact]
    public void SpendingIsStillAllowed()
    {
        Assert.True(ProgressGuard.Allows("geo", new JValue(94), new JValue(0)));
        Assert.True(ProgressGuard.Allows("ShellShards", new JValue(515), new JValue(400)));
    }

    [Fact]
    public void RoomListsOnlyGrow()
    {
        JArray host = ["Tut_01", "Bone_01"];
        JArray reset = ["Tut_01"];
        JToken merged = ProgressGuard.Merge("scenesMapped", host, reset);
        Assert.Equal(["Tut_01", "Bone_01"], merged.Select(room => (string)room!));
    }
}
