using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace SilksongCoop.Rules;

/// <summary>One changed leaf; <see cref="New"/> null means the leaf is gone.</summary>
internal sealed record LeafChange(string Path, JToken? Old, JToken? New);

/// <summary>
/// Diffs and patches JSON at leaf granularity, so two games editing different parts of one save field
/// (two tools' ammo, two quests) don't overwrite each other. Arrays of objects with a string "Name" are
/// keyed by it (the game's SerializableNamedList); other arrays are one leaf.
/// </summary>
internal static class JsonLeaves
{
    public const char Separator = '\u001f';
    private const string KeyMark = "#";
    private const string NameKey = "Name";

    public static Dictionary<string, JToken> Flatten(string root, JToken token)
    {
        Dictionary<string, JToken> leaves = [];
        Walk(root, token, leaves);
        return leaves;
    }

    /// <summary>Removals first, deepest first; then additions and edits, shallowest first.</summary>
    public static List<LeafChange> Diff(Dictionary<string, JToken> before, Dictionary<string, JToken> after)
    {
        // An empty list gaining its first entry turns from a leaf into a container; it is not removed.
        IEnumerable<LeafChange> removed = before
            .Where(pair => !after.ContainsKey(pair.Key) && !HasChildren(after, pair.Key))
            .OrderByDescending(pair => Depth(pair.Key))
            .Select(pair => new LeafChange(pair.Key, pair.Value, null));
        IEnumerable<LeafChange> changed = after
            .Where(pair => !Kept(before, pair.Key, pair.Value))
            .OrderBy(pair => Depth(pair.Key))
            .Select(pair => new LeafChange(pair.Key, before.GetValueOrDefault(pair.Key), pair.Value));
        return [.. removed, .. changed];
    }

    /// <summary>
    /// Applies a change below the root token (the first path segment names the root) and returns the root,
    /// which is replaced when it was missing or a scalar. Additive turns an integer edit into a delta.
    /// </summary>
    public static JToken? Apply(JToken? root, LeafChange change, bool additive)
    {
        Patch patch = new(change.Path.Split(Separator), change, additive);
        return patch.SetAt(root, 1);
    }

    private static bool HasChildren(Dictionary<string, JToken> leaves, string path)
    {
        string prefix = path + Separator;
        return leaves.Keys.Any(key => key.StartsWith(prefix, System.StringComparison.Ordinal));
    }

    private static bool Kept(Dictionary<string, JToken> before, string key, JToken now)
    {
        return before.TryGetValue(key, out JToken? old)
            && JToken.DeepEquals(old, now);
    }

    public static string RootOf(string path)
    {
        int end = path.IndexOf(Separator);
        return end < 0 ? path : path.Substring(0, end);
    }

    private static void Walk(string path, JToken token, Dictionary<string, JToken> leaves)
    {
        if (token is JObject { HasValues: true } obj)
        {
            foreach (JProperty property in obj.Properties())
            {
                Walk(path + Separator + property.Name, property.Value, leaves);
            }
            return;
        }
        if (token is JArray array && IsKeyed(array))
        {
            foreach (JObject element in array.Cast<JObject>())
            {
                string elementPath = path + Separator + KeyMark + (string)element[NameKey]!;
                leaves[elementPath] = JValue.CreateNull();
                foreach (JProperty property in element.Properties().Where(p => p.Name != NameKey))
                {
                    Walk(elementPath + Separator + property.Name, property.Value, leaves);
                }
            }
            return;
        }
        leaves[path] = token;
    }

    private static bool IsKeyed(JArray array) => array.Count > 0 && array.All(IsNamed);

    private static bool IsNamed(JToken item) => item is JObject element && IsString(element[NameKey]);

    private static bool IsString(JToken? token) => token?.Type == JTokenType.String;

    private static int Depth(string path) => path.Count(c => c == Separator);

    private sealed class Patch(string[] segments, LeafChange change, bool additive)
    {
        private bool Removes => change.New is null;

        public JToken? SetAt(JToken? node, int index)
        {
            if (index == segments.Length)
            {
                return Removes ? null : Combine(node);
            }
            return segments[index].StartsWith(KeyMark) ? SetInKeyed(node, index) : SetInObject(node, index);
        }

        private JToken? SetInObject(JToken? node, int index)
        {
            if (node is not JObject obj)
            {
                if (Removes)
                {
                    return node;
                }
                obj = [];
            }
            string key = segments[index];
            JToken? child = obj[key];
            JToken? updated = SetAt(child, index + 1);
            if (updated is null)
            {
                obj.Remove(key);
            }
            else if (!ReferenceEquals(updated, child))
            {
                obj[key] = updated;
            }
            return obj;
        }

        private JToken? SetInKeyed(JToken? node, int index)
        {
            if (node is not JArray array)
            {
                if (Removes)
                {
                    return node;
                }
                array = [];
            }
            string name = segments[index].Substring(KeyMark.Length);
            JObject? element = array.OfType<JObject>().FirstOrDefault(item => (string?)item[NameKey] == name);
            bool isMarker = index == segments.Length - 1;
            if (Removes && (isMarker || element is null))
            {
                element?.Remove();
                return array;
            }
            if (element is null)
            {
                element = new JObject { [NameKey] = name };
                array.Add(element);
            }
            if (!isMarker)
            {
                SetInObject(element, index + 1);
            }
            return array;
        }

        private JToken Combine(JToken? current)
        {
            bool integers = current?.Type == JTokenType.Integer
                && change.Old?.Type == JTokenType.Integer
                && change.New?.Type == JTokenType.Integer;
            if (additive && integers)
            {
                return new JValue((long)current! + ((long)change.New! - (long)change.Old!));
            }
            return change.New!.DeepClone();
        }
    }
}
