using System;
using System.Collections.Generic;

namespace SilksongCoop.Rules;

/// <summary>A door of my room: the room it leads to and the gate it comes out at there.</summary>
internal sealed record Door(string TargetScene, string EntryPoint);

/// <summary>Where the partner pointer aims and how strongly it shows.</summary>
internal static class Pointer
{
    /// <summary>World units past the screen edge at which the pointer reaches full strength.</summary>
    public const float FadeLength = 4f;

    /// <summary>The door here that comes out where the partner came in; -1 when none does.</summary>
    public static int DoorTo(IReadOnlyList<Door> doors, string partnerScene, string partnerEntry)
    {
        if (partnerEntry.Length == 0)
        {
            return -1;
        }
        for (int i = 0; i < doors.Count; i++)
        {
            if (doors[i].TargetScene == partnerScene && doors[i].EntryPoint == partnerEntry)
            {
                return i;
            }
        }
        return -1;
    }

    /// <summary>Hidden while the target is on screen, then fading in farther past the edge.</summary>
    public static float Alpha(float pastScreenEdge) => Math.Clamp(pastScreenEdge / FadeLength, 0f, 1f);
}
