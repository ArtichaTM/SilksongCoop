using SilksongCoop.Rules;

namespace SilksongCoop.Tests;

public class PointerTests
{
    private static readonly Door[] Doors =
    [
        new("B", "left1"),
        new("C", "right1"),
        new("B", "top1"),
    ];

    [Fact]
    public void OfTwoDoorsToOneRoomPicksTheOneUsed() => Assert.Equal(2, Pointer.DoorTo(Doors, "B", "top1"));

    [Theory]
    [InlineData("B", "bench")]
    [InlineData("D", "left1")]
    [InlineData("B", "")]
    public void NoMatchingDoorHidesThePointer(string scene, string entry) =>
        Assert.Equal(-1, Pointer.DoorTo(Doors, scene, entry));

    [Theory]
    [InlineData(0f, 0f)]
    [InlineData(-1f, 0f)]
    [InlineData(Pointer.FadeLength / 2, 0.5f)]
    [InlineData(Pointer.FadeLength * 3, 1f)]
    public void FadesInPastTheScreenEdge(float past, float alpha) => Assert.Equal(alpha, Pointer.Alpha(past));
}
