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
            coop.Replays.OnSwitch(__instance, toState);
            coop.Pickups.OnSwitch(__instance, toState);
        }
    }

    /// <summary>A synced FSM's random float: owner and mirror both take the owner's roll.</summary>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(RandomFloat), nameof(RandomFloat.OnEnter))]
    private static bool RandomFloatEnter(RandomFloat __instance)
    {
        if (InCoop is not { } coop || !coop.Enemies.TakeRoll(__instance, out float value))
        {
            return true;
        }
        __instance.storeResult.Value = value;
        __instance.Finish();
        return false;
    }

    /// <summary>A synced FSM's either-or float (a volley's side): the owner's pick, as above.</summary>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(RandomFloatEither), nameof(RandomFloatEither.OnEnter))]
    private static bool RandomFloatEitherEnter(RandomFloatEither __instance)
    {
        if (InCoop is not { } coop || !coop.Enemies.TakeRoll(__instance, out float value))
        {
            return true;
        }
        __instance.storeResult.Value = value;
        __instance.Finish();
        return false;
    }

    /// <summary>A mirror takes only its hero's hits; allies and hazards hit the owner's (7.4 a).</summary>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(HealthManager), nameof(HealthManager.Hit))]
    private static bool HitBefore(
        HealthManager __instance,
        HitInstance hitInstance,
        ref IHitResponder.HitResponse __result,
        out int __state)
    {
        __state = -1;
        if (InCoop is not { } coop || !coop.Enemies.IsMirrored(__instance))
        {
            return true;
        }
        if (!hitInstance.IsHeroDamage)
        {
            __result = IHitResponder.Response.None;
            return false;
        }
        __state = __instance.hp;
        EnemySync.LocalHit = true;
        return true;
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

    /// <summary>Corpses wait under their enemy and move to the scene root on death (2.37).</summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(EnemyDeathEffects), nameof(EnemyDeathEffects.PreInstantiate))]
    private static void CorpsesMade(EnemyDeathEffects __instance)
    {
        foreach (GameObject corpse in __instance.instantiatedCorpses ?? [])
        {
            if (corpse)
            {
                EnemySync.KeepId(corpse);
            }
        }
    }

    /// <summary>Outside coop too: a room's spawns are made as it loads, before anyone joins.</summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(PreInstantiateGameObject), nameof(PreInstantiateGameObject.Awake))]
    private static void PreInstantiated(PreInstantiateGameObject __instance) =>
        Coop.Current?.Enemies.OnPreInstantiated(__instance);

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

/// <summary>
/// A mirrored enemy faces the way the owner's does (its scale comes with each frame). Its own facing actions
/// would turn it toward this game's hero and play a turn animation, then the owner's scale flips it back.
/// </summary>
[HarmonyPatch]
internal static class MirrorFacingPatches
{
    private static readonly string[] Prefixes = ["FaceDirection", "FaceObject", "FlipScale"];

    private static IEnumerable<MethodBase> TargetMethods()
    {
        string[] hooks = ["OnEnter", "OnUpdate", "OnLateUpdate", "OnFixedUpdate"];
        return typeof(FaceDirection).Assembly.GetTypes()
            .Where(type => type.Namespace == typeof(FaceDirection).Namespace && Faces(type))
            .SelectMany(type => hooks.Select(hook => Hook(type, hook)))
            .Where(method => method is not null && method.DeclaringType != typeof(FsmStateAction))
            .Distinct()!;
    }

    private static MethodInfo? Hook(Type type, string name) =>
        type.GetMethod(name, BindingFlags.Public | BindingFlags.Instance);

    /// <summary>Scale flips only; FaceObjectTk2dSpriteScale flips the sprite, which frames lack.</summary>
    private static bool Faces(Type type) =>
        Prefixes.Any(type.Name.StartsWith) && type.Name != nameof(FaceObjectTk2dSpriteScale);

    private static bool Prefix(FsmStateAction __instance) =>
        Coop.Current is not { InCoop: true } coop || !coop.Enemies.MirrorsFacing(__instance.Fsm);
}
