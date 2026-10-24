using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using HutongGames.PlayMaker;
using SilksongCoop.Net;
using UnityEngine;

namespace SilksongCoop.Game;

/// <summary>
/// Diagnostics for purchases the mod does not sync (user note 1): every rosary or shard spend is matched to
/// a known sync (shop, toll, tool crafting, quest turn-in) and logged; an unmatched one is an error in both
/// games' logs, with the FSM or the code that took the currency.
/// </summary>
internal sealed class SpendLog(Coop coop) : Module(coop)
{
    private const float MatchWindow = 1.5f;

    // Callers whose spend needs nothing from us: the loadout is per player, quest progress is in the save.
    private static readonly (string Caller, string Kind)[] LocalCallers =
    [
        (nameof(ToolItem), "tool refill"), (nameof(ToolItemManager), "tool craft"),
        (nameof(QuestTargetCurrency), "quest turn-in"),
    ];

    private readonly List<Spend> _pending = [];
    private float _lastShopPurchase = float.NegativeInfinity;

    public override void Register(Session session) => session.On(MessageType.CurrencySpent, OnPartnerSpent);

    public override void OnLeft() => _pending.Clear();

    /// <summary>A shop item was bought here; spends around it are that purchase.</summary>
    public void OnShopPurchase() => _lastShopPurchase = Time.unscaledTime;

    public void OnSpent(int amount, CurrencyType type)
    {
        if (Coop.InCoop && amount > 0)
        {
            (string where, string? kind) = Describe();
            _pending.Add(new Spend(amount, type, Coop.Rooms.Current ?? "-", where, kind, Time.unscaledTime));
        }
    }

    /// <summary>A shop takes the currency before or after it marks the item bought; wait for both.</summary>
    public override void Update()
    {
        float now = Time.unscaledTime;
        foreach (Spend spend in _pending.Where(spend => now - spend.At >= MatchWindow).ToList())
        {
            _pending.Remove(spend);
            bool shop = Mathf.Abs(_lastShopPurchase - spend.At) <= MatchWindow;
            string? kind = spend.Kind ?? (shop ? "shop" : null);
            Report("this game", spend.Amount, spend.Type, spend.Scene, spend.Where, kind);
            Session?.Send(MessageType.CurrencySpent, w => w
                .Int(spend.Amount)
                .Int((int)spend.Type)
                .String(spend.Scene)
                .String(spend.Where)
                .String(kind ?? ""));
        }
    }

    private void OnPartnerSpent(PacketReader reader)
    {
        int amount = reader.Int();
        CurrencyType type = (CurrencyType)reader.Int();
        string scene = reader.String();
        string where = reader.String();
        string kind = reader.String();
        Report(Coop.Partner.Name, amount, type, scene, where, kind.Length == 0 ? null : kind);
    }

    private void Report(string who, int amount, CurrencyType type, string scene, string where, string? kind)
    {
        string text = $"spend: {who} spent {amount} {type} in {scene} via {where}";
        if (kind is null)
        {
            Coop.Log.LogError($"{text}; the mod does not sync what it bought");
        }
        else
        {
            Coop.Log.LogInfo($"{text} ({kind})");
        }
    }

    /// <summary>The FSM (object path and state) or the game class that took the currency.</summary>
    private static (string Where, string? Kind) Describe()
    {
        string[] callers = new StackTrace(3, false).GetFrames()
            .Select(frame => frame.GetMethod()?.DeclaringType?.Name ?? "")
            .Where(name => name.Length > 0)
            .ToArray();
        string? kind = LocalCallers.FirstOrDefault(entry => callers.Contains(entry.Caller)).Kind;
        if (FsmExecutionStack.ExecutingFsm is { GameObject: { } go } fsm)
        {
            bool toll = go.GetComponents<PlayMakerFSM>().Any(other => other.FsmName == TollSync.UnlockFsm);
            kind ??= toll ? "toll, partner's machine unlocks" : null;
            return ($"{GameRefs.PathOf(go.transform)} [{fsm.Name}] {fsm.ActiveStateName}", kind);
        }
        string caller = callers.FirstOrDefault(name => !IsPlumbing(name)) ?? "unknown";
        return (caller, kind);
    }

    private static bool IsPlumbing(string type)
    {
        bool currency = type is nameof(CurrencyManager) or nameof(HeroController) or nameof(SpendLog);
        return currency || type.Contains("Patch");
    }

    private readonly record struct Spend(
        int Amount,
        CurrencyType Type,
        string Scene,
        string Where,
        string? Kind,
        float At);
}
