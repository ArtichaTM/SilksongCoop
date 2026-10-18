using System.Collections.Generic;
using System.Linq;
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
/// <remarks>
/// KLUDGE: this copies what the game draws, sprite by sprite (tk2d and plain Unity sprites, with colour
/// and material), instead of running the partner's moves. Anything the game shows another way (particle
/// systems, mesh effects, shaders driven by script) does not reach the partner. It costs ~40 bytes per
/// sprite per frame, and each new kind of visual tends to need its own capture rule (attached effects,
/// spawned skills). Thrown tools already left this path: <see cref="ToolReplicas"/> runs the real prefab
/// on both games. The same for the hero is the clean fix: a second hero driven by the partner's inputs.
/// </remarks>
internal sealed class HeroSync(Coop coop) : Module(coop)
{
    private const float SendInterval = 1f / 60f;
    private const int MaxEffects = 32;
    private const int MaxTools = 32;
    private const float RecacheInterval = 2f;

    // The local screen's darkness with a hole at the hero; drawn for the partner it darkens our Hornet.
    private const string VignettePath = "/Vignette:";

    // Cuts light into the room's darkness layer; drawn as a sprite it is a coloured aura (Shaman crest).
    private const string DarknessMaterial = "Sprites-Darkness";

    // About 36 bytes a frame: parts stay near 1 KB, under the internet's ~1.4 KB per datagram.
    private const int FramesPerPart = 28;

    private static readonly HashSet<DamageEnemies> ToolDamagers = [];

    private readonly List<(int Key, Renderer Renderer)> _effects = [];
    private readonly List<SpriteFrame> _effectFrames = [];
    private readonly List<SpriteFrame> _toolFrames = [];
    private readonly HashSet<Transform> _toolRoots = [];
    private HeroController? _capturedHero;
    private float _nextRecache;
    private int _heroChildren;
    private bool _capWarned;
    private string _bodyState = "";
    private int _bodyReports;
    private readonly HashSet<string> _reported = [];
    private byte _sentSequence;
    private byte _receiving;
    private int _partsMissing;
    private readonly List<SpriteFrame> _partEffects = [];
    private readonly List<SpriteFrame> _partTools = [];
    private float _nextSend;
    private Puppet? _puppet;

    /// <summary>The drawn partner, for enemy targeting and the downed camera; null if not shown.</summary>
    public Transform? PartnerTransform => _puppet is { Visible: true } puppet ? puppet.Root : null;

    public override void Register(Session session)
    {
        session.On(MessageType.HeroFrame, OnFrame);
        session.On(MessageType.HeroParts, OnParts);
    }

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
        // Effects can be parented to the hero after it spawned (crest switches, pooled skill effects).
        bool reparented = hero.transform.childCount != _heroChildren;
        if (hero != _capturedHero || reparented || Time.unscaledTime >= _nextRecache)
        {
            _nextRecache = Time.unscaledTime + RecacheInterval;
            CacheEffects(hero);
        }
        tk2dSprite body = hero.GetComponent<tk2dSprite>();
        bool shown = hero.gameObject.activeInHierarchy && body.GetComponent<MeshRenderer>().enabled;
        ReportBody(shown, body);
        _effectFrames.Clear();
        foreach ((int key, Renderer renderer) in _effects)
        {
            if (!renderer || !TryFrame(key, renderer, out SpriteFrame frame))
            {
                continue;
            }
            if (_effectFrames.Count < MaxEffects)
            {
                _effectFrames.Add(frame);
            }
            else if (!_capWarned)
            {
                _capWarned = true;
                Coop.Log.LogWarning($"over {MaxEffects} hero effects at once; {renderer.name} not sent");
            }
        }
        CaptureTools();
        SpriteFrame bodyFrame = SpriteFrame.Of(0, body);
        byte sequence = ++_sentSequence;
        int effectParts = Parts(_effectFrames.Count);
        int parts = effectParts + Parts(_toolFrames.Count);
        Session!.SendFast(MessageType.HeroFrame, w =>
        {
            w.Int(Coop.Rooms.CurrentHash).Bool(shown).Byte(sequence).Byte((byte)parts);
            bodyFrame.Write(w);
        });
        for (int part = 0; part < parts; part++)
        {
            bool tools = part >= effectParts;
            List<SpriteFrame> source = tools ? _toolFrames : _effectFrames;
            int start = (tools ? part - effectParts : part) * FramesPerPart;
            List<SpriteFrame> chunk = source.GetRange(start, Mathf.Min(FramesPerPart, source.Count - start));
            Session.SendFast(MessageType.HeroParts, w =>
            {
                w.Byte(sequence).Bool(tools);
                WriteFrames(w, chunk);
            });
        }
    }

    private static int Parts(int frames) => (frames + FramesPerPart - 1) / FramesPerPart;

    private static bool IsDrawn(tk2dBaseSprite sprite, Renderer renderer)
    {
        return sprite && sprite.gameObject.activeInHierarchy && renderer.enabled
            && sprite.Collection && Visible(sprite.color);
    }

    /// <summary>Diagnostic for a vanishing partner body: visibility and sheet changes, bounded.</summary>
    private void ReportBody(bool shown, tk2dSprite body)
    {
        string sheet = body.Collection ? body.Collection.name : "none";
        string state = $"body shown={shown} sheet={sheet} a={body.color.a:0.00}";
        if (state != _bodyState && _bodyReports < 100)
        {
            _bodyState = state;
            _bodyReports++;
            Coop.Log.LogInfo(state);
        }
    }

    /// <summary>Faded-out effects stay enabled; sent, they would crowd out the visible ones.</summary>
    private static bool Visible(Color color) => color.a > 0.01f;

    private void CaptureTools()
    {
        _toolFrames.Clear();
        _toolRoots.Clear();
        ToolDamagers.RemoveWhere(damager => !damager);
        foreach (DamageEnemies damager in ToolDamagers)
        {
            // Thrown tools run for real on the partner's side (ToolReplicas); copies here aren't ours.
            if (!ToolReplicas.IsOwnTool(damager.gameObject) && !ToolReplicas.IsReplica(damager))
            {
                _toolRoots.Add(ToolRoot(damager.transform));
            }
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
            if (IsDarkness(renderer) || !TryFrame(renderer.GetInstanceID(), renderer, out SpriteFrame frame))
            {
                continue;
            }
            if (_toolFrames.Count < MaxTools)
            {
                ReportToolPart(root, renderer);
                _toolFrames.Add(frame);
            }
            else if (_reported.Add("cap"))
            {
                Coop.Log.LogWarning($"over {MaxTools} tool sprites at once; {root.name} cut");
            }
        }
    }

    /// <summary>Diagnostic: each part of a thrown object the first time it is sent.</summary>
    private void ReportToolPart(Transform root, Renderer renderer)
    {
        if (_reported.Add($"tool {root.name}/{renderer.name}"))
        {
            string kind = renderer is SpriteRenderer ? "sprite" : "tk2d";
            Coop.Log.LogInfo($"sending tool part {root.name}/{renderer.name} ({kind})");
        }
    }

    /// <summary>A drawn tk2d or Unity sprite as a frame; false for anything else or faded out.</summary>
    private static bool TryFrame(int key, Renderer renderer, out SpriteFrame frame)
    {
        frame = default;
        if (!renderer.enabled || !renderer.gameObject.activeInHierarchy)
        {
            return false;
        }
        if (renderer is SpriteRenderer { sprite: not null } flat)
        {
            frame = SpriteFrame.Of(key, flat);
            return Visible(flat.color);
        }
        if (renderer.TryGetComponent(out tk2dBaseSprite sprite) && IsDrawn(sprite, renderer))
        {
            frame = SpriteFrame.Of(key, sprite);
            return true;
        }
        return false;
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
        _heroChildren = hero.transform.childCount;
        _effects.Clear();
        // Plain Unity sprites too: some moves hide the body and draw Hornet with them (wind-ups, charges).
        int heroPath = GameRefs.PathOf(hero.transform).Length;
        foreach (Renderer renderer in hero.GetComponentsInChildren<Renderer>(true))
        {
            bool sprite = renderer is SpriteRenderer || renderer.GetComponent<tk2dBaseSprite>();
            string path = GameRefs.PathOf(renderer.transform).Substring(heroPath);
            if (renderer.gameObject != hero.gameObject && sprite && !path.StartsWith(VignettePath)
                && !IsDarkness(renderer))
            {
                _effects.Add((Fnv.Hash(path + renderer.GetType().Name), renderer));
            }
        }
    }

    private static bool IsDarkness(Renderer renderer)
    {
        Material material = renderer.sharedMaterial;
        return material && material.name.StartsWith(DarknessMaterial);
    }

    private void OnFrame(PacketReader reader)
    {
        int room = reader.Int();
        bool shown = reader.Bool();
        byte sequence = reader.Byte();
        int parts = reader.Byte();
        SpriteFrame body = SpriteFrame.Read(reader);
        Coop.Partner.Position = body.Position;
        if (room != Coop.Rooms.CurrentHash || Coop.Rooms.Current is null)
        {
            _puppet?.Hide();
            return;
        }
        _puppet ??= new Puppet(Coop.Partner.Name, Coop.Config.Tint.Value);
        _puppet.SetTint(Coop.Config.Tint.Value);
        // Some moves (tool throws, Plasmium) hide the body and draw Hornet with an attached sprite.
        bool down = Coop.Partner.Downed;
        _puppet.ShowBody(shown && !down, !down, body);
        _receiving = sequence;
        _partsMissing = parts;
        _partEffects.Clear();
        _partTools.Clear();
        if (parts == 0)
        {
            _puppet.ShowExtras(_partEffects, _partTools);
        }
    }

    /// <summary>A frame's effects arrive in parts; a frame missing any keeps the last complete set.</summary>
    private void OnParts(PacketReader reader)
    {
        byte sequence = reader.Byte();
        bool tools = reader.Bool();
        List<SpriteFrame> frames = ReadFrames(reader);
        if (sequence != _receiving || _partsMissing <= 0 || _puppet is null)
        {
            return;
        }
        (tools ? _partTools : _partEffects).AddRange(frames);
        if (--_partsMissing == 0)
        {
            _puppet.ShowExtras(_partEffects, _partTools);
        }
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
            bool spawned = __instance.representingTool || __instance.isHeroDamage;
            bool thrown = spawned && !__instance.GetComponentInParent<HeroController>();
            if (thrown)
            {
                ToolDamagers.Add(__instance);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("OnDisable")]
        private static void Disabled(DamageEnemies __instance) => ToolDamagers.Remove(__instance);
    }
}
