using System;
using System.Collections.Generic;
using System.Linq;
using SilksongCoop.Net;
using SilksongCoop.Rules;
using UnityEngine.SceneManagement;

namespace SilksongCoop.Game;

/// <summary>
/// Which room each player is in, and who simulates it (D9, D12): the host whenever the host is there,
/// otherwise whoever is.
/// </summary>
internal sealed class Rooms(Coop coop) : Module(coop)
{
    /// <summary>My room once the hero stands in it; null while loading or in menus.</summary>
    public string? Current { get; private set; }

    /// <summary>Where this game is headed while it loads a room.</summary>
    public string? Heading { get; private set; }

    public int CurrentHash => Current is null ? 0 : Fnv.Hash(Current);

    public bool PartnerHere => Coop.InCoop && Current is not null && Coop.Partner.Scene == Current;

    /// <summary>In one room, or one of the two still loading into the other's.</summary>
    public bool Together
    {
        get
        {
            Partner partner = Coop.Partner;
            bool arriving = Current is null && Heading is not null && Heading == partner.Scene;
            bool partnerLoading = partner.Scene is null && partner.Heading is not null;
            bool partnerArriving = partnerLoading && partner.Heading == Current;
            return Coop.InCoop && (PartnerHere || arriving || partnerArriving);
        }
    }

    /// <summary>This game runs the enemies and objects of its room.</summary>
    public bool LocalOwns => !PartnerHere || Coop.IsHost;

    /// <summary>The room's own scene, or one it loaded on top (a boss arena such as Bone_05_boss).</summary>
    public bool Holds(Scene scene)
    {
        if (Current is null || !scene.IsValid())
        {
            return false;
        }
        if (scene.name == Current)
        {
            return true;
        }
        string name = scene.name;
        return SceneAdditiveLoadConditional._additiveSceneLoads
            .Any(load => load && load.gameObject.scene.name == Current && load.SceneNameToLoad == name);
    }

    /// <summary>Raised once the hero stands in a new room; the argument is the scene name.</summary>
    public event Action<string>? Entered;

    public override void Register(Session session) => session.On(MessageType.RoomInfo, OnRoomInfo);

    public override void Update()
    {
        string? scene = GameRefs.GameplayScene;
        if (scene is null)
        {
            Current = null;
            return;
        }
        if (scene != Current && GameRefs.HeroReady)
        {
            Current = scene;
            Heading = null;
            Entered?.Invoke(scene);
            SendRoomInfo();
        }
    }

    public override void OnJoined() => SendRoomInfo();

    /// <summary>Called as a scene transition starts: the partner owns its room until we arrive.</summary>
    public void Leaving(string targetScene)
    {
        Current = null;
        Heading = targetScene;
        Session?.Send(MessageType.RoomInfo, w => w.String("").String(targetScene).String(""));
    }

    private void SendRoomInfo()
    {
        if (Current is null || !Coop.InCoop)
        {
            return;
        }
        List<TransitionPoint> points = TransitionPoint.TransitionPoints ?? [];
        List<string> gates = points.Where(p => p).Select(p => p.name).Distinct().ToList();
        List<string> neighbours = points
            .Where(p => p && !string.IsNullOrEmpty(p.targetScene))
            .Select(p => p.targetScene)
            .Distinct()
            .ToList();
        string entryGate = GameRefs.Manager?.entryGateName ?? "";
        Session?.Send(MessageType.RoomInfo, w =>
        {
            w.String(Current).String(Current).String(entryGate);
            WriteList(w, gates);
            WriteList(w, neighbours);
        });
    }

    private void OnRoomInfo(PacketReader reader)
    {
        Partner partner = Coop.Partner;
        string scene = reader.String();
        partner.Heading = reader.String();
        partner.EntryGate = reader.String();
        if (scene.Length == 0)
        {
            partner.Scene = null;
            return;
        }
        partner.Gates = ReadList(reader);
        partner.Neighbours = ReadList(reader);
        partner.Scene = scene;
        if (scene == Current)
        {
            // We were here first: hand over (host arriving) or align the mirror (friend arriving).
            Coop.Enemies.SendSnapshot();
            Coop.Breakables.SendSnapshot();
        }
    }

    private static void WriteList(PacketWriter writer, List<string> items)
    {
        writer.Int(items.Count);
        foreach (string item in items)
        {
            writer.String(item);
        }
    }

    private static List<string> ReadList(PacketReader reader)
    {
        int count = reader.Int();
        List<string> items = new(count);
        for (int i = 0; i < count; i++)
        {
            items.Add(reader.String());
        }
        return items;
    }
}
