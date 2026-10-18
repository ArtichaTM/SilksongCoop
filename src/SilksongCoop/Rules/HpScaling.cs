using System;

namespace SilksongCoop.Rules;

/// <summary>D22, D41, D50: enemy HP multipliers while a friend is connected.</summary>
internal static class HpScaling
{
    public static int Scale(int hp, float factor) => Math.Max(1, (int)Math.Round(hp * factor));

    /// <summary>Swaps an applied factor for another, e.g. regular to boss at the boss title.</summary>
    public static int Rescale(int hp, float applied, float wanted) => Scale(hp, wanted / applied);

    /// <summary>Fallback for bosses that never show a title: they are all tanky.</summary>
    public static bool LooksLikeBoss(int baseHp, int threshold) => threshold > 0 && baseHp >= threshold;
}
