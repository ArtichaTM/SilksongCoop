using SilksongCoop.Rules;

namespace SilksongCoop.Tests;

public class TeleportTableTests
{
    private const string Header = "id\trow\telement\tscene\tload\tdoor\tpath\tx\ty\n";

    private static readonly string Tsv = Header
        + "02.01\tBonetown.2\tBreakable Wall\tBonetown\tBonetown\tbot1\tWorld/Breakable Wall\t265.2\t7.0\n"
        + "03.03\tBone_01.5\tBone Flyer\tBone_01\tBone_01\tright1\tWave 1/Bone Flyer (1)\t109.4\t24.7\n"
        + "03.03\tBone_01.5\tBone Flyer\tBone_01\tBone_01\tright1\tWave 3/Bone Flyer (3)\t85.6\t31.2\n"
        + "03.03\tBone_01.6\tBone Goomba\tBone_01\tBone_01\tright1\tBone Goomba\t\t\n"
        + "03.29\tBone_05_boss.4\tBone Beast\tBone_05_boss\tBone_05\tbot1\t\t\t\n";

    private static List<TeleportRow> Rows() => TeleportTable.Rows(TeleportTable.Parse(Tsv));

    [Fact]
    public void RowsGroupAVerdictsObjectsPerScene()
    {
        TeleportRow flyers = Rows()[1];
        Assert.Equal(("03.03", "Bone_01", 3), (flyers.Id, flyers.Scene, flyers.Objects.Count));
        Assert.Equal("03.03  Bone_01.5, Bone_01.6  Bone Flyer; Bone Goomba", flyers.Label);
    }

    [Fact]
    public void EmptyCellsHaveNoPosition()
    {
        TeleportRow flyers = Rows()[1];
        Assert.Equal(265.2f, Rows()[0].Objects[0].X);
        Assert.Null(flyers.Objects[2].X);
    }

    [Fact]
    public void AdditiveSceneLoadsThroughItsParent()
    {
        TeleportRow beast = Rows()[2];
        Assert.Equal(("Bone_05", "bot1"), (beast.Load, beast.Door));
        Assert.Empty(beast.Objects);
    }

    [Theory]
    [InlineData("02.01", true)]
    [InlineData("bone_01", false)]
    [InlineData("breakable", true)]
    [InlineData("bonetown", true)]
    [InlineData("", true)]
    public void FilterMatchesIdRowNameOrScene(string query, bool matches) =>
        Assert.Equal(matches, TeleportTable.Matches(Rows()[0], query));

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(0, 1)]
    [InlineData(2, 0)]
    public void NearStepsThroughObjectsAndWraps(int current, int next) =>
        Assert.Equal(next, TeleportTable.Next(Rows()[1], current));

    [Fact]
    public void RowWithoutObjectsHasNoNext() => Assert.Equal(-1, TeleportTable.Next(Rows()[2], -1));

    [Fact]
    public void FileIsTheIdsVerdictFile() => Assert.Equal("03", TeleportTable.File("03.29"));
}
