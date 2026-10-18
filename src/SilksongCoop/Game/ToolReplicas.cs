using System.Collections.Generic;
using HutongGames.PlayMaker;
using SilksongCoop.Net;
using UnityEngine;

namespace SilksongCoop.Game;

/// <summary>
/// Thrown tools run for real on both games (D45): the thrower sends which tool left the hand and how, the
/// partner spawns the same prefab from the game's pool, so its FSM, particles, explosion and materials are
/// the game's own. The copy, and everything it spawns, does no damage there; only the thrower's tool hits.
/// </summary>
internal sealed class ToolReplicas(Coop coop) : Module(coop)
{
    private const string ToolPrefix = "Tool ";
    private const string ToolFsm = "Tool Attacks";

    // Copies of the partner's tools and what they spawned: harmless here.
    private static readonly HashSet<GameObject> Replicas = [];

    // This player's tools and what they spawned: shown on the partner's side by its own copy.
    private static readonly HashSet<GameObject> Own = [];
    private static readonly List<(GameObject Tool, string Prefab)> Thrown = [];
    private static bool _inThrow;
    private static bool _spawningReplica;
    private readonly HashSet<string> _logged = [];

    public override void Register(Session session) => session.On(MessageType.ToolThrown, OnThrown);

    /// <summary>Sent a frame after the spawn, once the game has set the tool's velocity and scale.</summary>
    public override void LateUpdate()
    {
        foreach ((GameObject tool, string prefab) in Thrown)
        {
            if (tool && tool.activeInHierarchy && Coop.Rooms.PartnerHere)
            {
                Send(tool, prefab);
            }
        }
        Thrown.Clear();
        Replicas.RemoveWhere(go => !go || !go.activeInHierarchy);
        Own.RemoveWhere(go => !go || !go.activeInHierarchy);
    }

    public override void OnLeft() => Thrown.Clear();

    public static void BeginThrow() => _inThrow = true;

    public static void EndThrow() => _inThrow = false;

    /// <summary>Every pool spawn: copies inherit harmlessness, own tools get reported.</summary>
    public static void OnSpawned(GameObject prefab, GameObject spawned)
    {
        if (_spawningReplica || InTree(Replicas, Running()))
        {
            Replicas.Add(spawned);
            return;
        }
        if (InTree(Own, Running()))
        {
            Own.Add(spawned);
            return;
        }
        if (prefab.name.StartsWith(ToolPrefix) && (_inThrow || HeroToolFsmRunning()))
        {
            Own.Add(spawned);
            Thrown.Add((spawned, prefab.name));
        }
    }

    public static void OnRecycled(GameObject go)
    {
        Replicas.Remove(go);
        Own.Remove(go);
    }

    public static bool IsReplica(Component component) => InTree(Replicas, component.gameObject);

    /// <summary>Part of this player's tool: the partner's own copy shows and sounds it.</summary>
    public static bool IsOwnTool(GameObject? go) => InTree(Own, go);

    private static GameObject? Running() => FsmExecutionStack.ExecutingFsm?.GameObject;

    private static bool HeroToolFsmRunning()
    {
        Fsm? fsm = FsmExecutionStack.ExecutingFsm;
        bool tools = fsm is not null && fsm.Name == ToolFsm;
        return tools && GameRefs.Hero is { } hero && fsm!.GameObject == hero.gameObject;
    }

    private static bool InTree(HashSet<GameObject> roots, GameObject? go)
    {
        if (roots.Count == 0 || go is null || !go)
        {
            return false;
        }
        for (Transform? at = go.transform; at != null; at = at.parent)
        {
            if (roots.Contains(at.gameObject))
            {
                return true;
            }
        }
        return false;
    }

    private void Send(GameObject tool, string prefab)
    {
        if (_logged.Add("send " + prefab))
        {
            Coop.Log.LogInfo($"tool copy sent: {prefab}");
        }
        Transform transform = tool.transform;
        Vector2 velocity = tool.TryGetComponent(out Rigidbody2D body) ? body.linearVelocity : Vector2.zero;
        Vector3 scale = transform.localScale;
        Session?.Send(MessageType.ToolThrown, w => w
            .String(prefab)
            .Float(transform.position.x)
            .Float(transform.position.y)
            .Float(transform.position.z)
            .Float(transform.eulerAngles.z)
            .Float(scale.x)
            .Float(scale.y)
            .Float(velocity.x)
            .Float(velocity.y));
    }

    private void OnThrown(PacketReader reader)
    {
        string name = reader.String();
        Vector3 position = new(reader.Float(), reader.Float(), reader.Float());
        float angle = reader.Float();
        Vector2 scale = new(reader.Float(), reader.Float());
        Vector2 velocity = new(reader.Float(), reader.Float());
        if (!Coop.Rooms.PartnerHere)
        {
            return;
        }
        if (Coop.Preload.Prefab(name) is not { } prefab)
        {
            Coop.Log.LogWarning($"partner threw {name}, which is not loaded here");
            return;
        }
        GameObject copy;
        _spawningReplica = true;
        try
        {
            copy = ObjectPool.Spawn(prefab, null, position, Quaternion.Euler(0f, 0f, angle), false);
        }
        finally
        {
            _spawningReplica = false;
        }
        if (!copy)
        {
            return;
        }
        Replicas.Add(copy);
        if (_logged.Add("spawn " + name))
        {
            Coop.Log.LogInfo($"tool copy spawned: {name}");
        }
        copy.transform.localScale = new Vector3(scale.x, scale.y, copy.transform.localScale.z);
        if (copy.TryGetComponent(out Rigidbody2D body))
        {
            body.linearVelocity = velocity;
        }
    }
}
