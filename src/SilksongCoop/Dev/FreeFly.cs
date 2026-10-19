using HarmonyLib;
using SilksongCoop.Game;
using UnityEngine;

namespace SilksongCoop.Dev;

/// <summary>
/// Flying through terrain. Unlike the game's own noclip the collider stays on as a trigger, so the
/// hero still enters the areas that start events; a cutscene taking control ends the flight.
/// </summary>
internal sealed class FreeFly(CoopConfig config)
{
    private const float DashBoost = 2.5f;

    private static FreeFly? _active;
    private HeroController? _hero;

    public bool Flying => _hero is not null;

    /// <summary>Only with dev tools enabled; runs after the hero's FixedUpdate sets its velocity.</summary>
    public static void Patch(Harmony harmony)
    {
        harmony.Patch(
            AccessTools.Method(typeof(HeroController), "FixedUpdate"),
            postfix: new HarmonyMethod(typeof(FreeFly), nameof(AfterFixedUpdate)));
    }

    /// <summary>False when the hero cannot fly now (not in control).</summary>
    public bool Toggle()
    {
        if (Flying)
        {
            Stop();
            return true;
        }
        if (!GameRefs.HeroInControl || GameRefs.Hero is not { } hero)
        {
            return false;
        }
        _hero = hero;
        _active = this;
        hero.col2d.isTrigger = true;
        hero.AffectedByGravity(false);
        return true;
    }

    public void Stop()
    {
        if (_hero)
        {
            _hero!.col2d.isTrigger = false;
            _hero.AffectedByGravity(true);
        }
        _hero = null;
        if (_active == this)
        {
            _active = null;
        }
    }

    private static void AfterFixedUpdate(HeroController __instance)
    {
        if (_active is { } fly && fly._hero == __instance)
        {
            fly.Steer(__instance);
        }
    }

    private void Steer(HeroController hero)
    {
        if (hero.controlReqlinquished || !hero.acceptingInput)
        {
            Stop();
            return;
        }
        HeroActions input = GameManager.instance.inputHandler.inputActions;
        Vector2 direction = new(Axis(input.Left, input.Right), Axis(input.Down, input.Up));
        float speed = config.FlySpeed.Value * (input.Dash.IsPressed ? DashBoost : 1f);
        hero.rb2d.linearVelocity = direction.normalized * speed;
    }

    private static float Axis(InControl.PlayerAction negative, InControl.PlayerAction positive) =>
        (positive.IsPressed ? 1f : 0f) - (negative.IsPressed ? 1f : 0f);
}
