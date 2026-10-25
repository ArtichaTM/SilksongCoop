using UnityEngine;

namespace SilksongCoop.Game;

/// <summary>The game's singletons without their "create or complain" side effects.</summary>
internal static class GameRefs
{
    public static GameManager? Manager => GameManager.SilentInstance ? GameManager.SilentInstance : null;

    public static HeroController? Hero => HeroController._instance ? HeroController._instance : null;

    public static bool InMenu => Manager is { } gm && gm.IsMenuScene();

    /// <summary>The gameplay scene the hero stands in, or null in menus and cutscene-only scenes.</summary>
    public static string? GameplayScene
    {
        get
        {
            GameManager? gm = Manager;
            return gm is not null && Hero is not null && gm.IsGameplayScene() ? gm.sceneName : null;
        }
    }

    /// <summary>Hero placed in the scene and no transition running.</summary>
    public static bool HeroReady
    {
        get
        {
            GameManager? gm = Manager;
            HeroController? hero = Hero;
            return gm is not null && hero is not null && hero.isHeroInPosition && !gm.IsInSceneTransition;
        }
    }

    /// <summary>Ready and steering: not on a bench, in a cutscene or mid-respawn.</summary>
    public static bool HeroInControl
    {
        get
        {
            HeroController? hero = Hero;
            bool free = hero is not null && hero.acceptingInput && !hero.controlReqlinquished;
            return free && HeroReady && !hero!.cState.dead;
        }
    }

    /// <summary>Hierarchy path with sibling indices so twins differ; the same in both games.</summary>
    /// <param name="root">An ancestor the path starts below; null for the scene root.</param>
    public static string PathOf(Transform transform, Transform? root = null)
    {
        string path = $"{transform.name}:{transform.GetSiblingIndex()}";
        for (Transform parent = transform.parent; parent != null && parent != root; parent = parent.parent)
        {
            path = $"{parent.name}:{parent.GetSiblingIndex()}/{path}";
        }
        return path;
    }
}
