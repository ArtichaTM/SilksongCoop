using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HutongGames.PlayMaker;
using UnityEngine;

namespace SilksongCoop.Game;

/// <summary>
/// Both loadouts drawable on both games: every crest, silk skill and tool sprite collection the hero could
/// use is forced to load (tk2d loads platform data lazily, on first local use) once per hero in coop.
/// </summary>
internal sealed class AssetPreload(Coop coop) : Module(coop)
{
    private const BindingFlags Fields = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    private readonly HashSet<tk2dSpriteCollectionData> _loaded = [];
    private readonly HashSet<GameObject> _visited = [];
    private readonly Dictionary<string, GameObject> _prefabs = [];
    private HeroController? _preloadedFor;

    public override void Update()
    {
        if (!Coop.InCoop || GameRefs.Hero is not { } hero || hero == _preloadedFor)
        {
            return;
        }
        _preloadedFor = hero;
        _visited.Clear();
        Visit(hero.gameObject);
        foreach (PlayMakerFSM fsm in hero.GetComponentsInChildren<PlayMakerFSM>(true))
        {
            VisitSpawns(fsm.Fsm);
        }
        foreach (tk2dSpriteAnimation library in AnimationLibraries(hero))
        {
            VisitClips(library);
        }
        foreach (ToolItem tool in ToolItemManager.GetAllTools())
        {
            if (tool && tool.Usage.ThrowPrefab is { } prefab && prefab)
            {
                Visit(prefab);
            }
        }
        Coop.Log.LogInfo($"preloaded {_loaded.Count} sprite collections for both loadouts");
    }

    public override void OnLeft() => _preloadedFor = null;

    /// <summary>A prefab the hero or a tool can spawn, by name: what partner tool copies come from.</summary>
    public GameObject? Prefab(string name) => _prefabs.TryGetValue(name, out GameObject? at) ? at : null;

    /// <summary>Every animation Hornet can play: her animator, crest overrides, the windy set.</summary>
    private static IEnumerable<tk2dSpriteAnimation> AnimationLibraries(HeroController hero)
    {
        foreach (tk2dSpriteAnimator animator in hero.GetComponentsInChildren<tk2dSpriteAnimator>(true))
        {
            yield return animator.Library;
        }
        foreach (HeroController.ConfigGroup group in hero.configs.Concat(hero.specialConfigs))
        {
            yield return group.Config ? group.Config.heroAnimOverrideLib : null!;
        }
        yield return hero.animCtrl.windyAnimLib;
    }

    /// <summary>Sheets an animation switches to mid-clip (a throw pose) load lazily like any other.</summary>
    private void VisitClips(tk2dSpriteAnimation? library)
    {
        if (library is null || !library || library.clips is null)
        {
            return;
        }
        foreach (tk2dSpriteAnimationClip clip in library.clips)
        {
            foreach (tk2dSpriteAnimationFrame frame in clip?.frames ?? [])
            {
                if (frame.spriteCollection is { } collection && collection && _loaded.Add(collection))
                {
                    Load(collection);
                }
            }
        }
    }

    /// <summary>Prefabs the hero's FSMs spawn (silk skills, crest effects) live in action fields.</summary>
    private void VisitSpawns(Fsm fsm)
    {
        foreach (FsmState state in fsm.States)
        {
            foreach (FsmStateAction action in state.Actions)
            {
                foreach (FieldInfo field in action.GetType().GetFields(Fields))
                {
                    object? value = field.GetValue(action);
                    GameObject? prefab = value switch
                    {
                        GameObject go => go,
                        FsmGameObject variable => variable.Value,
                        _ => null,
                    };
                    if (prefab && !prefab!.scene.IsValid())
                    {
                        Visit(prefab);
                    }
                }
            }
        }
    }

    private void Visit(GameObject root)
    {
        if (!_visited.Add(root))
        {
            return;
        }
        _prefabs.TryAdd(root.name, root);
        foreach (tk2dBaseSprite sprite in root.GetComponentsInChildren<tk2dBaseSprite>(true))
        {
            if (sprite.Collection is { } collection && collection && _loaded.Add(collection))
            {
                Load(collection);
            }
        }
    }

    private void Load(tk2dSpriteCollectionData collection)
    {
        tk2dSpriteCollectionData loaded = collection.inst;
        Material? first = loaded.materials is { Length: > 0 } materials ? materials[0] : null;
        bool textured = first && first!.mainTexture;
        if (!textured)
        {
            Coop.Log.LogWarning($"sprite collection {collection.name} has no texture after loading");
        }
    }
}
