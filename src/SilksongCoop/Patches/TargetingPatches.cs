using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using SilksongCoop.Game;
using UnityEngine;

namespace SilksongCoop.Patches;

/// <summary>
/// H14: enemies and shared objects chase, face and measure the nearer player in both games (a flea cage
/// checks how far its hitter stands). Each targeting action reads the hero from an FSM variable; for the
/// length of the call the variable points at the chosen player and is put back afterwards, so nothing in the
/// FSM changes for good. A measurement may hold the hero in either end (Bone Circler measures from the hero
/// to its home).
/// </summary>
[HarmonyPatch]
internal static class TargetingPatches
{
    private const float Rethink = 3f;

    private static readonly string[] Ends = ["gameObject", "target"];

    private static readonly (Type Action, string Method, string[] Fields)[] Actions =
    [
        (typeof(ChaseObject), "DoBuzz", ["target"]),
        (typeof(ChaseObjectGround), "DoChase", ["target"]),
        (typeof(ChaseObjectV2), "DoChase", ["target"]),
        (typeof(ChaseObjectV3), "DoChase", ["target"]),
        (typeof(ChaseObjectVertical), "DoChase", ["target"]),
        (typeof(CheckTargetDirection), "DoCheckDirection", Ends),
        (typeof(DistanceFly), "DoBuzz", ["target"]),
        (typeof(DistanceFlyV2), "DoBuzz", ["target"]),
        (typeof(FaceObject), "DoFace", ["objectB"]),
        (typeof(FaceObjectV2), "DoFace", ["objectB"]),
        (typeof(FaceObjectV3), "DoFace", ["ObjectB"]),
        (typeof(FaceObjectV4), "DoFace", ["ObjectB"]),
        (typeof(GetDistance), "DoGetDistance", Ends),
        (typeof(GetXDistance), "DoGetDistance", Ends),
        (typeof(GetYDistance), "DoGetDistance", Ends),
        (typeof(GetAngleToTarget2D), "DoGetAngle", Ends),
        (typeof(FireAtTarget), "DoSetVelocity", ["target"]),
        // Skarr hunters dig to the teleplane nearest the hero and aim from its position (marks 9, 11-12).
        (typeof(FindClosest), "DoFindClosest", ["gameObject"]),
        (typeof(GetPosition), "DoGetPosition", ["gameObject"]),
    ];

    private static readonly Dictionary<Type, FieldInfo[]> Fields = Actions.ToDictionary(
        entry => entry.Action,
        entry => entry.Fields.Select(field => AccessTools.Field(entry.Action, field)).ToArray());

    private static readonly Dictionary<int, (bool Partner, float Until)> Choices = [];

    private static IEnumerable<MethodBase> TargetMethods() => Actions.Select(e => Method(e.Action, e.Method));

    private static MethodBase Method(Type action, string name) => AccessTools.Method(action, name);

    private static void Prefix(FsmStateAction __instance, out Swap __state)
    {
        __state = default;
        if (Coop.Current is not { InCoop: true } coop || !coop.Rooms.PartnerHere)
        {
            return;
        }
        if (GameRefs.Hero is not { } hero || coop.Heroes.PartnerTransform is not { } partner)
        {
            return;
        }
        FsmGameObject? variable = HeroVariable(__instance, hero.gameObject);
        if (variable is null || coop.Enemies.ModeOf(__instance.Fsm) == FsmMode.None)
        {
            return;
        }
        if (PrefersPartner(__instance.Fsm.GameObject, hero.transform, partner))
        {
            __state = new Swap { Variable = variable, Hero = hero.gameObject };
            variable.Value = partner.gameObject;
        }
    }

    private static void Postfix(Swap __state)
    {
        if (__state.Variable is { } variable)
        {
            variable.Value = __state.Hero;
        }
    }

    /// <summary>The action's first listed parameter that holds the hero.</summary>
    private static FsmGameObject? HeroVariable(FsmStateAction action, GameObject hero)
    {
        foreach (FieldInfo field in Fields[action.GetType()])
        {
            if (VariableOf(field.GetValue(action)) is { } variable && variable.Value == hero)
            {
                return variable;
            }
        }
        return null;
    }

    /// <summary>An object parameter, or an owner parameter set to a specific object.</summary>
    private static FsmGameObject? VariableOf(object? parameter)
    {
        return parameter switch
        {
            FsmGameObject value => value,
            FsmOwnerDefault { OwnerOption: OwnerDefaultOption.SpecifyGameObject } owner => owner.GameObject,
            _ => null,
        };
    }

    private struct Swap
    {
        public FsmGameObject? Variable;
        public GameObject? Hero;
    }

    /// <summary>Nearest player, re-decided every few seconds so enemies don't flicker between two.</summary>
    private static bool PrefersPartner(GameObject enemy, Transform hero, Transform partner)
    {
        int key = enemy.GetInstanceID();
        float now = Time.time;
        if (Choices.TryGetValue(key, out (bool Partner, float Until) choice) && now < choice.Until)
        {
            return choice.Partner;
        }
        Vector2 at = enemy.transform.position;
        bool partnerNearer = Vector2.Distance(at, partner.position) < Vector2.Distance(at, hero.position);
        Choices[key] = (partnerNearer, now + Rethink);
        return partnerNearer;
    }
}
