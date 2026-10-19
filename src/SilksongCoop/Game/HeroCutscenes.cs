using System.Collections.Generic;
using System.Linq;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using SilksongCoop.Rules;

namespace SilksongCoop.Game;

/// <summary>FSMs that take or give back the local hero's control: each game runs its own.</summary>
internal static class HeroCutscenes
{
    /// <summary>Unstarted FSMs count as false unless <paramref name="loadActions"/> (logs errors).</summary>
    public static bool Drives(Fsm fsm, bool loadActions = false)
    {
        if (!fsm.Initialized && !loadActions)
        {
            return false;
        }
        return ActionsOf(fsm).Any(action => action switch
        {
            HeroControllerMethods call => HeroControl.IsControlCall(call.method.ToString()),
            CallMethodProper call => call.behaviour.Value == nameof(HeroController)
                && HeroControl.IsControlCall(call.methodName.Value),
            SendMessage message => HeroControl.IsControlCall(message.functionCall?.FunctionName),
            HeroRelinquishControlDynamic => true,
            _ => false,
        });
    }

    /// <summary>Every action of every state; a state whose actions fail to load counts as empty.</summary>
    public static IEnumerable<FsmStateAction> ActionsOf(Fsm fsm)
    {
        foreach (FsmState state in fsm.States)
        {
            FsmStateAction[] actions;
            try
            {
                actions = state.Actions ?? [];
            }
            catch (System.Exception)
            {
                actions = [];
            }
            foreach (FsmStateAction action in actions)
            {
                yield return action;
            }
        }
    }
}
