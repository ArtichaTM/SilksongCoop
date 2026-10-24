using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GlobalEnums;
using HarmonyLib;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using SilksongCoop.Game;
using UnityEngine;

namespace SilksongCoop.Patches;

/// <summary>H11-H13: room simulation by the owner, hits by the hitter, HP scaling.</summary>
[HarmonyPatch]
internal static class EnemyPatches
{
    private static Coop? InCoop => Coop.Current is { InCoop: true } coop ? coop : null;

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Fsm), nameof(Fsm.ProcessEvent))]
    private static bool ProcessEvent(Fsm __instance, FsmEvent fsmEvent)
    {
        if (InCoop is not { } coop)
        {
            return true;
        }
        if (coop.Enemies.BlocksEvent(__instance))
        {
            coop.Enemies.RelayBlocked(__instance, fsmEvent);
            return false;
        }
        return !coop.Waits.HoldsEvent(__instance, fsmEvent);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Fsm), nameof(Fsm.SwitchState))]
    private static void SwitchState(Fsm __instance, FsmState toState)
    {
        if (InCoop is { } coop && toState is not null)
        {
            coop.Enemies.OnSwitch(__instance, toState);
            coop.Platforms.OnSwitch(__instance, toState);
            coop.Pulls.OnFsmSwitch(__instance, toState);
            coop.Memories.OnSwitch(__instance, toState);
            coop.BellBeast.OnSwitch(__instance, toState);
            coop.Tolls.OnSwitch(__instance, toState);
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(HealthManager), nameof(HealthManager.Hit))]
    private static void HitBefore(HealthManager __instance, out int __state)
    {
        __state = -1;
        if (InCoop is { } coop && coop.Enemies.IsMirrored(__instance))
        {
            __state = __instance.hp;
            EnemySync.LocalHit = true;
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(HealthManager), nameof(HealthManager.Hit))]
    private static void HitAfter(HealthManager __instance, HitInstance hitInstance, int __state)
    {
        if (__state < 0)
        {
            return;
        }
        EnemySync.LocalHit = false;
        int damage = __instance.isDead ? __state : __state - __instance.hp;
        InCoop?.Enemies.ReportHit(__instance, damage, hitInstance);
    }

    [HarmonyPrefix]
    [HarmonyPatch(
        typeof(HealthManager),
        nameof(HealthManager.Die),
        [
            typeof(float?), typeof(AttackTypes), typeof(NailElements), typeof(GameObject),
            typeof(bool), typeof(float), typeof(bool), typeof(bool),
        ]
    )]
    private static bool DieBefore(HealthManager __instance, out bool __state)
    {
        __state = __instance.isDead;
        bool ownDeath = EnemySync.LocalHit || EnemySync.RemoteDeath;
        return ownDeath || InCoop?.Enemies.IsMirrored(__instance) != true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(
        typeof(HealthManager),
        nameof(HealthManager.Die),
        [
            typeof(float?), typeof(AttackTypes), typeof(NailElements), typeof(GameObject),
            typeof(bool), typeof(float), typeof(bool), typeof(bool),
        ]
    )]
    private static void DieAfter(HealthManager __instance, bool __state)
    {
        bool justDied = !__state && __instance.isDead && !EnemySync.RemoteDeath;
        if (justDied && InCoop is { } coop && coop.Enemies.IsOwnedAndShared(__instance))
        {
            coop.Enemies.ReportDeath(__instance);
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(HealthManager), "OnEnable")]
    private static void Enabled(HealthManager __instance) => InCoop?.Enemies.Track(__instance);

    [HarmonyPostfix]
    [HarmonyPatch(typeof(HealthManager), nameof(HealthManager.OnAwake))]
    private static void Awoke(HealthManager __instance, bool __result)
    {
        if (__result && InCoop is { } coop)
        {
            Scaling.OnSpawn(__instance, coop.Config);
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(DisplayBossTitle), nameof(DisplayBossTitle.OnEnter))]
    private static void BossTitle(DisplayBossTitle __instance)
    {
        GameObject go = __instance.Fsm.GameObject;
        HealthManager? health = go.GetComponent<HealthManager>();
        if (!health)
        {
            health = go.GetComponentInChildren<HealthManager>(true);
        }
        if (!health)
        {
            health = go.GetComponentInParent<HealthManager>(true);
        }
        if (health && InCoop is { } coop)
        {
            Scaling.OnBossTitle(health!, coop.Config);
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(BattleScene), nameof(BattleScene.StartBattle))]
    private static void BattleStart(BattleScene __instance) => InCoop?.Enemies.ReportBattleStart(__instance);

    /// <summary>4.1 b: a hero walking into a battle with the partner away holds its start back.</summary>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(BattleScene), "OnTriggerEnter2D")]
    private static bool BattleEntered(BattleScene __instance, Collider2D collision)
    {
        bool hero = collision.CompareTag("Player") && collision.GetComponent<HeroController>();
        return !hero || InCoop?.Waits.HoldsBattle(__instance) != true;
    }
}

/// <summary>Marks an FSM while it checks its own body against the ground; a mirror never relays it.</summary>
[HarmonyPatch]
internal static class BodyCheckPatches
{
    private static readonly Type[] Checks =
    [
        typeof(CheckIsCharacterGrounded), typeof(CheckCollisionSide), typeof(CheckCollisionSideV2),
        typeof(CheckCollisionSideEnter), typeof(CheckCollisionSideEnterV2), typeof(CheckGroundClearance),
        typeof(DetectGroundEdge),
    ];

    private static readonly string[] Steps =
        ["OnEnter", "OnUpdate", "OnFixedUpdate", "DoCollisionEnter2D", "DoCollisionStay2D"];

    private static IEnumerable<MethodBase> TargetMethods()
    {
        return Checks.SelectMany(type => Steps.Select(step => AccessTools.DeclaredMethod(type, step)))
            .OfType<MethodBase>();
    }

    private static void Prefix(FsmStateAction __instance, out Fsm? __state)
    {
        __state = EnemySync.BodyCheck;
        EnemySync.BodyCheck = __instance.Fsm;
    }

    private static void Finalizer(Fsm? __state) => EnemySync.BodyCheck = __state;
}
