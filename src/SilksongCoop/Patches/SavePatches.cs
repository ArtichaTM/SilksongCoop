using System;
using HarmonyLib;

namespace SilksongCoop.Patches;

/// <summary>
/// H8, C1: the friend's game plays the host's save in memory and writes nothing to disk. The callers'
/// callbacks still fire, as if the save succeeded, so death and quit flows don't wait forever. A save
/// the friend's game asks for (a bench) has the host save instead (10.09).
/// </summary>
[HarmonyPatch(typeof(GameManager))]
internal static class SavePatches
{
    private static bool IsFriend => Coop.Current?.IsFriend == true;

    [HarmonyPrefix]
    [HarmonyPatch(
        nameof(GameManager.SaveGame),
        [typeof(int), typeof(Action<bool>), typeof(bool), typeof(AutoSaveName)]
    )]
    private static bool SaveGame(Action<bool> ogCallback)
    {
        if (IsFriend)
        {
            Coop.Current?.World.RequestHostSave();
        }
        return Skip(ogCallback);
    }

    [HarmonyPrefix]
    [HarmonyPatch(
        nameof(GameManager.CreateRestorePoint),
        [typeof(int), typeof(AutoSaveName), typeof(Action<bool>)]
    )]
    private static bool RestorePoint(Action<bool> callback) => Skip(callback);

    [HarmonyPrefix]
    [HarmonyPatch(nameof(GameManager.SaveGameData))]
    private static bool SaveGameData(Action<bool> ogCallback) => Skip(ogCallback);

    private static bool Skip(Action<bool>? callback)
    {
        if (!IsFriend)
        {
            return true;
        }
        callback?.Invoke(true);
        return false;
    }
}
