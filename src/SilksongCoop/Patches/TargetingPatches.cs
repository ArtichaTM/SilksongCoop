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
/// H14: enemies the owner runs chase, face and measure the nearer player. Each targeting action reads the
/// hero from an FSM variable; for the length of the call the variable points at the chosen player and is
/// put back afterwards, so nothing in the FSM changes for good.
/// </summary>
[HarmonyPatch]
internal static class TargetingPatches
{
    private const float Rethink = 3f;

    private static readonly (Type Action, string Method, string Field)[] Actions =
    [
        (typeof(ChaseObject), "DoBuzz", "target"),
        (typeof(ChaseObjectGround), "DoChase", "target"),
        (typeof(ChaseObjectV2), "DoChase", "target"),
        (typeof(ChaseObjectV3), "DoChase", "target"),
        (typeof(ChaseObjectVertical), "DoChase", "target"),
        (typeof(CheckTargetDirection), "DoCheckDirection", "target"),
        (typeof(FaceObject), "DoFace", "objectB"),
        (typeof(FaceObjectV2), "DoFace", "objectB"),
        (typeof(FaceObjectV3), "DoFace", "ObjectB"),
        (typeof(FaceObjectV4), "DoFace", "ObjectB"),
        (typeof(GetDistance), "DoGetDistance", "target"),
        (typeof(GetXDistance), "DoGetDistance", "target"),
        (typeof(GetYDistance), "DoGetDistance", "target"),
        (typeof(GetAngleToTarget2D), "DoGetAngle", "target"),
    ];

    private static readonly Dictionary<Type, FieldInfo> Fields = Actions.ToDictionary(
        entry => entry.Action,
        entry => AccessTools.Field(entry.Action, entry.Field));

    private static readonly Dictionary<int, (bool Partner, float Until)> Choices = [];

    private static IEnumerable<MethodBase> TargetMethods() => Actions.Select(e => Method(e.Action, e.Method));

    private static MethodBase Method(Type action, string name) => AccessTools.Method(action, name);

    private static void Prefix(FsmStateAction __instance, out Swap __state)
    {
        __state = default;
        if (Coop.Current is not { InCoop: true } coop || !coop.Rooms.PartnerHere || !coop.Rooms.LocalOwns)
        {
            return;
        }
        if (GameRefs.Hero is not { } hero || coop.Heroes.PartnerTransform is not { } partner)
        {
            return;
        }
        FsmGameObject? variable = Fields[__instance.GetType()].GetValue(__instance) as FsmGameObject;
        if (variable?.Value != hero.gameObject || coop.Enemies.ModeOf(__instance.Fsm) != FsmMode.Authority)
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
