using System.Collections.Generic;
using HarmonyLib;
using SilksongCoop.Net;
using SilksongCoop.Rules;
using UnityEngine;

namespace SilksongCoop.Game;

/// <summary>
/// Visual parity (D36): every frame the local Hornet's sprites (body, attached effects such as needle
/// slashes and silk skills, and thrown tools per D45) go to the partner, who draws them on a
/// <see cref="Puppet"/>. Sprite-level copying is frame exact and needs no animation-clip bookkeeping.
/// </summary>
internal sealed class HeroSync(Coop coop) : Module(coop)
{
    private const float SendInterval = 1f / 60f;
    private const int MaxEffects = 32;
    private const int MaxTools = 32;

    private static readonly HashSet<DamageEnemies> ToolDamagers = [];

    private readonly List<(int Key, tk2dBaseSprite Sprite, Renderer Renderer)> _effects = [];
    private readonly List<SpriteFrame> _effectFrames = [];
    private readonly List<SpriteFrame> _toolFrames = [];
    private readonly HashSet<Transform> _toolRoots = [];
    private HeroController? _capturedHero;
    private float _nextSend;
    private Puppet? _puppet;

    /// <summary>The drawn partner, for enemy targeting and the downed camera; null if not shown.</summary>
    public Transform? PartnerTransform => _puppet is { Visible: true } puppet ? puppet.Root : null;

    public override void Register(Session session) => session.On(MessageType.HeroFrame, OnFrame);

    public override void LateUpdate()
    {
        if (!Coop.InCoop || Coop.Rooms.Current is null || GameRefs.Hero is not { } hero)
        {
            return;
        }
        if (Time.unscaledTime < _nextSend)
        {
            return;
        }
        _nextSend = Time.unscaledTime + SendInterval;
        Capture(hero);
    }

    public override void OnLeft() => DestroyPuppet();

    public override void Dispose() => DestroyPuppet();

    private void DestroyPuppet()
    {
        _puppet?.Destroy();
        _puppet = null;
    }

    private void Capture(HeroController hero)
    {
        if (hero != _capturedHero)
        {
            CacheEffects(hero);
        }
        tk2dSprite body = hero.GetComponent<tk2dSprite>();
        bool shown = hero.gameObject.activeInHierarchy && body.GetComponent<MeshRenderer>().enabled;
        _effectFrames.Clear();
        foreach ((int key, tk2dBaseSprite sprite, Renderer renderer) in _effects)
        {
            if (IsDrawn(sprite, renderer) && _effectFrames.Count < MaxEffects)
            {
                _effectFrames.Add(SpriteFrame.Of(key, sprite));
            }
        }
        CaptureTools();
        SpriteFrame bodyFrame = SpriteFrame.Of(0, body);
        Session!.SendFast(MessageType.HeroFrame, w =>
        {
            w.Int(Coop.Rooms.CurrentHash).Bool(shown);
            bodyFrame.Write(w);
            WriteFrames(w, _effectFrames);
            WriteFrames(w, _toolFrames);
        });
    }

    private static bool IsDrawn(tk2dBaseSprite sprite, Renderer renderer)
    {
        return sprite && sprite.gameObject.activeInHierarchy && renderer.enabled
            && sprite.Collection;
    }

    private void CaptureTools()
    {
        _toolFrames.Clear();
        _toolRoots.Clear();
        ToolDamagers.RemoveWhere(damager => !damager);
        foreach (DamageEnemies damager in ToolDamagers)
        {
            _toolRoots.Add(ToolRoot(damager.transform));
        }
        foreach (Transform root in _toolRoots)
        {
            CaptureTool(root);
        }
    }

    /// <summary>Everything drawn by one thrown tool: tk2d or plain Unity sprites, anywhere in it.</summary>
    private void CaptureTool(Transform root)
    {
        foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>())
        {
            if (_toolFrames.Count >= MaxTools || !renderer.enabled)
            {
                continue;
            }
            int key = renderer.GetInstanceID();
            if (renderer is SpriteRenderer { sprite: not null } flat)
            {
                _toolFrames.Add(SpriteFrame.Of(key, flat));
            }
            else if (renderer.TryGetComponent(out tk2dBaseSprite sprite) && sprite.Collection)
            {
                _toolFrames.Add(SpriteFrame.Of(key, sprite));
            }
        }
    }

    /// <summary>The spawned tool a damager belongs to: its nearest instantiated ancestor.</summary>
    private static Transform ToolRoot(Transform damager)
    {
        for (Transform? at = damager; at != null; at = at.parent)
        {
            if (at.name.EndsWith("(Clone)"))
            {
                return at;
            }
        }
        return damager;
    }

    private void CacheEffects(HeroController hero)
    {
        _capturedHero = hero;
        _effects.Clear();
        foreach (tk2dBaseSprite sprite in hero.GetComponentsInChildren<tk2dBaseSprite>(true))
        {
            if (sprite.gameObject == hero.gameObject)
            {
                continue;
            }
            string path = GameRefs.PathOf(sprite.transform).Substring(GameRefs.PathOf(hero.transform).Length);
            _effects.Add((Fnv.Hash(path), sprite, sprite.GetComponent<Renderer>()));
        }
    }

    private void OnFrame(PacketReader reader)
    {
        int room = reader.Int();
        bool shown = reader.Bool();
        SpriteFrame body = SpriteFrame.Read(reader);
        List<SpriteFrame> effects = ReadFrames(reader);
        List<SpriteFrame> tools = ReadFrames(reader);
        Coop.Partner.Position = body.Position;
        if (room != Coop.Rooms.CurrentHash || Coop.Rooms.Current is null)
        {
            _puppet?.Hide();
            return;
        }
        _puppet ??= new Puppet(Coop.Partner.Name, Coop.Config.Tint.Value);
        _puppet.SetTint(Coop.Config.Tint.Value);
        bool bodyShown = shown && !Coop.Partner.Downed;
        _puppet.Show(bodyShown, body, effects, tools);
    }

    private static void WriteFrames(PacketWriter writer, List<SpriteFrame> frames)
    {
        writer.Byte((byte)frames.Count);
        foreach (SpriteFrame frame in frames)
        {
            frame.Write(writer);
        }
    }

    private static List<SpriteFrame> ReadFrames(PacketReader reader)
    {
        int count = reader.Byte();
        List<SpriteFrame> frames = new(count);
        for (int i = 0; i < count; i++)
        {
            frames.Add(SpriteFrame.Read(reader));
        }
        return frames;
    }

    /// <summary>Tracks live tool damagers (projectiles, traps) the local player threw (D45).</summary>
    [HarmonyPatch(typeof(DamageEnemies))]
    private static class ToolDamagerPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("OnEnable")]
        private static void Enabled(DamageEnemies __instance)
        {
            bool thrown = __instance.representingTool && !__instance.GetComponentInParent<HeroController>();
            if (thrown && ToolDamagers.Add(__instance) && Coop.Current is { InCoop: true } coop)
            {
                coop.Log.LogInfo($"tracking thrown tool {__instance.name}");
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("OnDisable")]
        private static void Disabled(DamageEnemies __instance) => ToolDamagers.Remove(__instance);
    }
}
