using System.Collections.Generic;
using SilksongCoop.Rules;
using UnityEngine;

namespace SilksongCoop.Game;

/// <summary>What this game knows about the other player.</summary>
internal sealed class Partner
{
    public string Name { get; set; } = "";

    /// <summary>The room the partner stands in; null while they load a room or sit in a menu.</summary>
    public string? Scene { get; set; }

    /// <summary>Where a loading partner is headed.</summary>
    public string? Heading { get; set; }

    public string EntryGate { get; set; } = "";

    public List<string> Gates { get; set; } = [];

    public List<string> Neighbours { get; set; } = [];

    public bool Downed { get; set; }

    public Vector2 Cocoon { get; set; }

    public Vector3 Position { get; set; }

    /// <summary>Last known room for the tether; a loading partner counts as in the next room.</summary>
    public PartnerRoom? TetherRoom
    {
        get
        {
            if (Scene is not null)
            {
                return new PartnerRoom(Scene, Neighbours, Downed);
            }
            return Heading is null ? null : new PartnerRoom(Heading, [], Downed);
        }
    }

    public void Reset()
    {
        Name = "";
        Scene = null;
        Heading = null;
        EntryGate = "";
        Gates = [];
        Neighbours = [];
        Downed = false;
    }
}
