using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace SilksongCoop.Rules;

/// <summary>
/// The host's last line of defence for its save: progression only moves forward, whatever the friend's
/// game reports. Abilities stay learned, upgrades stay bought, visited and mapped rooms only accumulate.
/// </summary>
internal static class ProgressGuard
{
    private static readonly HashSet<string> GrowingNumbers =
    [
        "maxHealth", "maxHealthBase", "heartPieces", "silkMax", "silkRegenMax", "silkSpoolParts",
        "nailUpgrades", "attunementLevel", "silkSpecialLevel", "ToolPouchUpgrades", "ToolKitUpgrades",
    ];

    private static readonly HashSet<string> GrowingSets =
    [
        "scenesVisited", "scenesMapped", "scenesEncounteredBench", "scenesEncounteredCocoon",
    ];

    /// <summary>False when a friend's change to a whole field would take progression away.</summary>
    public static bool Allows(string field, JToken? current, JToken? proposed)
    {
        if (current is null || proposed is null)
        {
            return true;
        }
        bool learned = field.StartsWith("has", System.StringComparison.OrdinalIgnoreCase)
            || field.StartsWith("Unlocked");
        if (learned && current.Type == JTokenType.Boolean && proposed.Type == JTokenType.Boolean)
        {
            return (bool)proposed || !(bool)current;
        }
        bool numbers = current.Type == JTokenType.Integer && proposed.Type == JTokenType.Integer;
        return !(GrowingNumbers.Contains(field) && numbers && (long)proposed < (long)current);
    }

    /// <summary>Room lists merge as a union instead of being replaced.</summary>
    public static JToken Merge(string field, JToken? current, JToken proposed)
    {
        if (!GrowingSets.Contains(field) || current is not JArray known || proposed is not JArray offered)
        {
            return proposed;
        }
        IEnumerable<JToken> union = known.Concat(offered).Distinct(JToken.EqualityComparer);
        return new JArray(union);
    }
}
