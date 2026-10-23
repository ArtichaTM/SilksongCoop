using HarmonyLib;

namespace SilksongCoop.Patches;

/// <summary>Every way a prop breaks (hits, breakers, scripted) ends in <c>Breakable.Break</c>.</summary>
[HarmonyPatch(typeof(Breakable), nameof(Breakable.Break))]
internal static class BreakablePatches
{
    private static void Prefix(Breakable __instance, out int? __state) =>
        __state = Coop.Current is { InCoop: true } coop ? coop.Breakables.IdBeforeBreak(__instance) : null;

    private static void Postfix(Breakable __instance, int? __state)
    {
        if (__state is { } id)
        {
            Coop.Current?.Breakables.OnLocalBreak(__instance, id);
        }
    }
}
