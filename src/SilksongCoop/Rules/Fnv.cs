namespace SilksongCoop.Rules;

/// <summary>FNV-1a: a string hash both games agree on, unlike the per-process string.GetHashCode.</summary>
internal static class Fnv
{
    public static int Hash(string text)
    {
        unchecked
        {
            uint hash = 2166136261;
            foreach (char c in text)
            {
                hash = (hash ^ c) * 16777619;
            }
            return (int)hash;
        }
    }
}
