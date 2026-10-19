using Newtonsoft.Json.Linq;
using SilksongCoop.Rules;

namespace SilksongCoop.Tests;

public class JsonLeavesTests
{
    private const char S = JsonLeaves.Separator;

    private static JObject Tools(int pinAmmo, int bombAmmo)
    {
        JArray named = new(Tool("Pin", pinAmmo), Tool("Bomb", bombAmmo));
        return new JObject { ["savedData"] = named };
    }

    private static JObject Tool(string name, int ammo)
    {
        JObject data = new() { ["AmountLeft"] = ammo, ["IsUnlocked"] = true };
        return new JObject { ["Name"] = name, ["Data"] = data };
    }

    private static List<LeafChange> Diff(JToken before, JToken after)
    {
        Dictionary<string, JToken> old = JsonLeaves.Flatten("Tools", before);
        return JsonLeaves.Diff(old, JsonLeaves.Flatten("Tools", after));
    }

    [Fact]
    public void NamedListsDiffPerEntry()
    {
        LeafChange change = Assert.Single(Diff(Tools(5, 3), Tools(4, 3)));
        Assert.Equal($"Tools{S}savedData{S}#Pin{S}Data{S}AmountLeft", change.Path);
    }

    [Fact]
    public void HostAddsBothPlayersSpendingAsDeltas()
    {
        // Host threw one pin (5 -> 4) while the friend threw one too (its 5 -> 4).
        JToken host = Tools(4, 3);
        LeafChange friend = Assert.Single(Diff(Tools(5, 3), Tools(4, 3)));
        JToken merged = JsonLeaves.Apply(host, friend, additive: true)!;
        Assert.Equal(3, (int)merged["savedData"]![0]!["Data"]!["AmountLeft"]!);
    }

    [Fact]
    public void FriendAdoptsTheHostsValue()
    {
        LeafChange host = Assert.Single(Diff(Tools(5, 3), Tools(2, 3)));
        JToken merged = JsonLeaves.Apply(Tools(4, 3), host, additive: false)!;
        Assert.Equal(2, (int)merged["savedData"]![0]!["Data"]!["AmountLeft"]!);
    }

    [Fact]
    public void NewEntriesAppearAndGoneEntriesDisappear()
    {
        JObject before = Tools(5, 3);
        JObject after = Tools(5, 3);
        ((JArray)after["savedData"]!).RemoveAt(1);
        ((JArray)after["savedData"]!).Add(JObject.Parse("""{"Name": "Trap", "Data": {"AmountLeft": 1}}"""));
        JToken patched = before.DeepClone();
        foreach (LeafChange change in Diff(before, after))
        {
            patched = JsonLeaves.Apply(patched, change, additive: false)!;
        }
        string[] names = patched["savedData"]!.Select(entry => (string)entry["Name"]!).ToArray();
        Assert.Equal(["Pin", "Trap"], names);
        Assert.Equal(1, (int)patched["savedData"]![1]!["Data"]!["AmountLeft"]!);
    }

    [Fact]
    public void FirstEntryNeverDropsTheList()
    {
        // The game's named lists break on a missing savedData (first quest, first journal kill).
        JObject before = new() { ["savedData"] = new JArray() };
        JToken patched = before.DeepClone();
        foreach (LeafChange change in Diff(before, Tools(5, 3)))
        {
            patched = JsonLeaves.Apply(patched, change, additive: false)!;
            Assert.IsType<JArray>(patched["savedData"]);
        }
        Assert.True(JToken.DeepEquals(Tools(5, 3), patched));
    }

    [Fact]
    public void ScalarRootsAreOneLeaf()
    {
        LeafChange change = Assert.Single(JsonLeaves.Diff(
            JsonLeaves.Flatten("geo", new JValue(10)),
            JsonLeaves.Flatten("geo", new JValue(15))));
        Assert.Equal("geo", change.Path);
        Assert.Equal(25, (int)JsonLeaves.Apply(new JValue(20), change, additive: true)!);
    }

    [Fact]
    public void PlainArraysAreOneLeaf()
    {
        JArray before = ["Tut_01"];
        JArray after = ["Tut_01", "Bone_01"];
        LeafChange change = Assert.Single(JsonLeaves.Diff(
            JsonLeaves.Flatten("scenesVisited", before),
            JsonLeaves.Flatten("scenesVisited", after)));
        Assert.True(JToken.DeepEquals(after, JsonLeaves.Apply(before, change, additive: true)));
    }
}
