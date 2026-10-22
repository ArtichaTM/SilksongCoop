using SilksongCoop.Net;
using SilksongCoop.Rules;

namespace SilksongCoop.Tests;

public class RulesTests
{
    private static readonly PartnerRoom PartnerInB = new("B", ["A", "C"], Downed: false);

    [Theory]
    [InlineData("A", "B", TetherVerdict.Allowed)]
    [InlineData("A", "C", TetherVerdict.Allowed)]
    [InlineData("B", "D", TetherVerdict.PartnerTooFar)]
    [InlineData("A", "A", TetherVerdict.Allowed)]
    public void TetherKeepsPlayersWithinOneRoom(string from, string to, object expected) =>
        Assert.Equal((TetherVerdict)expected, Tether.Check(from, to, PartnerInB));

    [Fact]
    public void NobodyLeavesADownedPartner()
    {
        PartnerRoom downedHere = new("A", ["B"], Downed: true);
        Assert.Equal(TetherVerdict.PartnerDown, Tether.Check("A", "B", downedHere));
    }

    [Fact]
    public void NoPartnerNoTether() => Assert.Equal(TetherVerdict.Allowed, Tether.Check("A", "Z", null));

    [Fact]
    public void BossTitleSwapsRegularFactorForBossFactor()
    {
        int regular = HpScaling.Scale(100, 1.2f);
        Assert.Equal(120, regular);
        Assert.Equal(150, HpScaling.Rescale(regular, 1.2f, 1.5f));
    }

    [Fact]
    public void ScalingNeverKills() => Assert.Equal(1, HpScaling.Scale(1, 0.1f));

    [Fact]
    public void ThresholdZeroDisablesTheBossFallback() => Assert.False(HpScaling.LooksLikeBoss(5000, 0));

    [Theory]
    [InlineData("geo", FieldScope.Shared)]
    [InlineData("health", FieldScope.PerPlayer)]
    [InlineData("respawnScene", FieldScope.HostOwned)]
    [InlineData("ToolEquips", FieldScope.Loadout)]
    public void FieldScopes(string field, object scope) =>
        Assert.Equal((FieldScope)scope, FieldRules.Of(field));

    [Fact]
    public void OnlyTheHostSendsHostOwnedFields()
    {
        Assert.True(FieldRules.HostSends(FieldScope.HostOwned));
        Assert.False(FieldRules.FriendSends(FieldScope.HostOwned, sameRoom: false));
        Assert.False(FieldRules.HostSends(FieldScope.Loadout));
    }

    [Fact]
    public void FriendSendsHostOwnedFieldsInTheHostsRoom()
    {
        Assert.True(FieldRules.FriendSends(FieldScope.HostOwned, sameRoom: true));
        Assert.False(FieldRules.FriendSends(FieldScope.PerPlayer, sameRoom: true));
    }

    [Fact]
    public void FnvIsStable() => Assert.Equal(unchecked((int)0x811C9DC5), Fnv.Hash(""));

    [Fact]
    public void PacketRoundTrip()
    {
        byte[] data = new PacketWriter().Int(-5).String("héllo").Float(1.5f).Bool(true).ToArray();
        PacketReader reader = new(data);
        Assert.Equal(-5, reader.Int());
        Assert.Equal("héllo", reader.String());
        Assert.Equal(1.5f, reader.Float());
        Assert.True(reader.Bool());
        Assert.True(reader.AtEnd);
    }

    [Theory]
    [InlineData("RelinquishControl", true)]
    [InlineData("RegainControl", true)]
    [InlineData("StartAnimationControlToIdle", true)]
    [InlineData("FlipSprite", false)]
    [InlineData(null, false)]
    public void CutscenesAreKnownByHeroControlCalls(string? method, bool expected) =>
        Assert.Equal(expected, HeroControl.IsControlCall(method));
}
