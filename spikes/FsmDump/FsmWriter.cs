using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using HutongGames.PlayMaker;
using UnityEngine;

namespace FsmDump;

/// <summary>Plain-text dump of FSMs: states, actions with their field values, transitions.</summary>
internal static class FsmWriter
{
    public static int DumpAll(string path)
    {
        PlayMakerFSM[] fsms = Resources.FindObjectsOfTypeAll<PlayMakerFSM>()
            .Where(fsm => fsm.gameObject.scene.IsValid())
            .OrderBy(fsm => fsm.gameObject.scene.name)
            .ThenBy(fsm => PathOf(fsm.gameObject))
            .ToArray();
        using StreamWriter writer = new(path);
        foreach (PlayMakerFSM fsm in fsms)
        {
            Write(writer, fsm);
        }
        return fsms.Length;
    }

    public static string PathOf(GameObject? go)
    {
        if (go == null)
        {
            return "null";
        }
        string path = go.name;
        for (Transform parent = go.transform.parent; parent != null; parent = parent.parent)
        {
            path = parent.name + "/" + path;
        }
        return $"{go.scene.name}:{path}";
    }

    private static void Write(StreamWriter writer, PlayMakerFSM component)
    {
        Fsm fsm = component.Fsm;
        string active = component.isActiveAndEnabled ? "active" : "inactive";
        string where = PathOf(component.gameObject);
        writer.WriteLine($"FSM {where} [{fsm.Name}] {active}, now {fsm.ActiveStateName}");
        foreach (FsmTransition global in fsm.GlobalTransitions)
        {
            writer.WriteLine($"  global {global.EventName} -> {global.ToState}");
        }
        foreach (FsmState state in fsm.States)
        {
            writer.WriteLine($"  state {state.Name}");
            foreach (FsmStateAction action in SafeActions(state))
            {
                string enabled = action.Enabled ? "" : " (disabled)";
                writer.WriteLine($"    {action.GetType().Name}{enabled} {Fields(action)}");
            }
            foreach (FsmTransition transition in state.Transitions)
            {
                writer.WriteLine($"    on {transition.EventName} -> {transition.ToState}");
            }
        }
        writer.WriteLine();
    }

    private static FsmStateAction[] SafeActions(FsmState state)
    {
        try
        {
            return state.Actions;
        }
        catch (Exception)
        {
            return [];
        }
    }

    private static string Fields(FsmStateAction action)
    {
        FieldInfo[] fields = action.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
        string Pair(FieldInfo field) => $"{field.Name}={SafeFormat(() => field.GetValue(action))}";
        return string.Join(", ", fields.Select(Pair));
    }

    private static string SafeFormat(Func<object?> read)
    {
        try
        {
            return Format(read());
        }
        catch (Exception exception)
        {
            return $"<{exception.GetType().Name}>";
        }
    }

    private static string Format(object? value)
    {
        return value switch
        {
            null => "null",
            string text => $"\"{text}\"",
            FsmEvent fsmEvent => $"event:{fsmEvent.Name}",
            FsmOwnerDefault owner => $"owner:{owner.OwnerOption}/{Format(owner.GameObject)}",
            NamedVariable { IsNone: true } => "none",
            NamedVariable { UseVariable: true } variable => $"var:{variable.Name}",
            NamedVariable variable => Format(variable.RawValue),
            GameObject go => $"go:{PathOf(go)}",
            UnityEngine.Object asset when asset == null => "null",
            UnityEngine.Object asset => $"{asset.GetType().Name}:{asset.name}",
            IEnumerable items => $"[{string.Join(", ", items.Cast<object?>().Select(Format))}]",
            _ => value.ToString(),
        };
    }
}
