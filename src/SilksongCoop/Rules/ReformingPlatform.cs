using System.Collections.Generic;
using System.Linq;

namespace SilksongCoop.Rules;

/// <summary>One FSM transition: on <see cref="Event"/> go to the state named <see cref="To"/>.</summary>
internal readonly record struct FsmLink(string Event, string To);

/// <summary>
/// An FSM with "Break" and "Reform" states: a platform that crumbles when touched and builds itself back.
/// A break starts when the FSM leaves a resting state for a trigger: Break, or the antic finishing into it.
/// </summary>
internal sealed class ReformingPlatform
{
    private const string Break = "Break";
    private const string Reform = "Reform";
    private const string Finished = "FINISHED";

    private readonly HashSet<string> _triggers;
    private readonly HashSet<string> _rests;

    private ReformingPlatform(HashSet<string> triggers, HashSet<string> rests)
    {
        _triggers = triggers;
        _rests = rests;
    }

    /// <summary>The platform an FSM (states and their links) describes, or null for any other FSM.</summary>
    public static ReformingPlatform? Of(IReadOnlyDictionary<string, IReadOnlyList<FsmLink>> fsm)
    {
        if (!fsm.ContainsKey(Break) || !fsm.ContainsKey(Reform))
        {
            return null;
        }
        HashSet<string> triggers = [Break];
        triggers.UnionWith(fsm
            .Where(state => state.Value.Contains(new FsmLink(Finished, Break)))
            .Select(state => state.Key));
        HashSet<string> rests = fsm
            .Where(state => !triggers.Contains(state.Key) && LeadsToTrigger(state.Value))
            .Select(state => state.Key)
            .ToHashSet();
        return rests.Count == 0 ? null : new ReformingPlatform(triggers, rests);

        bool LeadsToTrigger(IReadOnlyList<FsmLink> links) => links.Any(link => triggers.Contains(link.To));
    }

    /// <summary>This switch starts a break.</summary>
    public bool Starts(string from, string to) => _rests.Contains(from) && _triggers.Contains(to);

    /// <summary>Whole and waiting: a partner's break may start here.</summary>
    public bool Rests(string state) => _rests.Contains(state);

    public bool IsTrigger(string state) => _triggers.Contains(state);
}
