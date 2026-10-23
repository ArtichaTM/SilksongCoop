using System.Collections.Generic;
using System.Linq;
using HutongGames.PlayMaker;
using SilksongCoop.Net;
using SilksongCoop.Rules;
using UnityEngine;

namespace SilksongCoop.Game;

/// <summary>
/// Platforms that crumble under a player and reform: each game runs its own, and whichever starts crumbling
/// first starts the partner's copy too. Its touch check needs the local collider, so no owner runs it.
/// </summary>
internal sealed class PlatformSync : Module
{
    private const float ReindexCooldown = 1f;

    private readonly Dictionary<Fsm, ReformingPlatform?> _shapes = [];
    private readonly Dictionary<(int Id, string Fsm), Fsm> _platforms = [];
    private float _lastIndex = -ReindexCooldown;
    private bool _applying;

    public PlatformSync(Coop coop)
        : base(coop) => coop.Rooms.Entered += OnEntered;

    private bool Enabled => Coop.Config.SyncPlatforms.Value;

    public override void Register(Session session) => session.On(MessageType.PlatformBreak, OnPartnerBreak);

    public override void Dispose() => Coop.Rooms.Entered -= OnEntered;

    /// <summary>A platform synced here; <see cref="EnemySync"/> leaves it alone.</summary>
    public bool Handles(Fsm fsm) => Enabled && ShapeOf(fsm) is not null;

    /// <summary>Called before any FSM switches state.</summary>
    public void OnSwitch(Fsm fsm, FsmState toState)
    {
        if (_applying || !Enabled || !Coop.Rooms.PartnerHere || fsm.ActiveState is not { } from)
        {
            return;
        }
        if (ShapeOf(fsm) is { } shape && shape.Starts(from.Name, toState.Name))
        {
            int id = EnemySync.IdOf(fsm.GameObject);
            Session?.Send(MessageType.PlatformBreak, w => w.Int(id).String(fsm.Name).String(toState.Name));
        }
    }

    private void OnEntered(string scene)
    {
        _shapes.Clear();
        _platforms.Clear();
        _lastIndex = -ReindexCooldown;
    }

    private void OnPartnerBreak(PacketReader reader)
    {
        int id = reader.Int();
        string fsmName = reader.String();
        string state = reader.String();
        if (!Enabled || Find(id, fsmName) is not { } fsm || ShapeOf(fsm) is not { } shape)
        {
            return;
        }
        bool shown = fsm.GameObject.activeInHierarchy;
        if (shown && fsm.ActiveState is { } now && shape.Rests(now.Name) && shape.IsTrigger(state))
        {
            _applying = true;
            try
            {
                fsm.SetState(state);
            }
            finally
            {
                _applying = false;
            }
        }
    }

    private ReformingPlatform? ShapeOf(Fsm fsm)
    {
        if (_shapes.TryGetValue(fsm, out ReformingPlatform? shape))
        {
            return shape;
        }
        GameObject go = fsm.GameObject;
        bool candidate = go && Coop.Rooms.Holds(go.scene) && !go.GetComponentInParent<HealthManager>(true);
        shape = candidate ? ReformingPlatform.Of(LinksOf(fsm)) : null;
        _shapes[fsm] = shape;
        if (shape is not null)
        {
            _platforms[(EnemySync.IdOf(go!), fsm.Name)] = fsm;
        }
        return shape;
    }

    private static Dictionary<string, IReadOnlyList<FsmLink>> LinksOf(Fsm fsm)
    {
        Dictionary<string, IReadOnlyList<FsmLink>> links = [];
        foreach (FsmState state in fsm.States)
        {
            links[state.Name] = state.Transitions.Select(t => new FsmLink(t.EventName, t.ToState)).ToList();
        }
        return links;
    }

    private Fsm? Find(int id, string fsmName)
    {
        if (!_platforms.TryGetValue((id, fsmName), out Fsm? fsm) || !fsm.GameObject)
        {
            Reindex();
            _platforms.TryGetValue((id, fsmName), out fsm);
        }
        return fsm is not null && fsm.GameObject ? fsm : null;
    }

    private void Reindex()
    {
        if (Time.unscaledTime - _lastIndex < ReindexCooldown || Coop.Rooms.Current is null)
        {
            return;
        }
        _lastIndex = Time.unscaledTime;
        foreach (PlayMakerFSM component in Resources.FindObjectsOfTypeAll<PlayMakerFSM>())
        {
            if (component.Fsm is { } fsm)
            {
                ShapeOf(fsm);
            }
        }
    }
}
