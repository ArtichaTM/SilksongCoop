using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GlobalEnums;
using HutongGames.PlayMaker;
using SilksongCoop.Net;
using SilksongCoop.Rules;
using UnityEngine;

namespace SilksongCoop.Game;

/// <summary>
/// Every other sound the local player makes (D36, D45): needle and skill sounds, thrown tools and
/// deployables. Any clip played on the player's objects, or by an FSM running on them, goes to the partner,
/// who plays the same clip, found by name, at the same spot.
/// </summary>
internal sealed class ClipAudioSync(Coop coop) : Module(coop)
{
    private const int Voices = 12;
    private const float RescanCooldown = 1f;

    private readonly Dictionary<int, AudioClip> _clips = [];
    private readonly Dictionary<int, bool> _toolObjects = [];
    private readonly HashSet<AudioSource> _movementSources = [];
    private readonly List<AudioSource> _voices = [];
    private HeroController? _movementOwner;
    private HeroController? _tablesOf;
    private readonly HashSet<RandomAudioClipTable> _heroTables = [];
    private GameObject? _speaker;
    private int _nextVoice;
    private float _lastScan = -RescanCooldown;

    public override void Register(Session session) => session.On(MessageType.ClipSound, OnClip);

    public override void Dispose()
    {
        if (_speaker)
        {
            Object.Destroy(_speaker);
        }
    }

    /// <summary>Called for every clip the game starts; forwards the local player's.</summary>
    public void OnPlayed(AudioSource source, AudioClip? clip, float volumeScale)
    {
        if (clip is null || !Coop.Rooms.PartnerHere || !IsOwn(source))
        {
            return;
        }
        int name = Fnv.Hash(clip.name);
        Vector3 at = source.transform.position;
        float volume = source.volume * volumeScale;
        float pitch = source.pitch;
        Session?.Send(MessageType.ClipSound, w => w
            .Int(name)
            .Float(at.x)
            .Float(at.y)
            .Float(volume)
            .Float(pitch));
    }

    /// <summary>Inside a play of one of the hero's own sound tables (rage attacks, voice lines).</summary>
    public static bool HeroTablePlaying { get; set; }

    /// <summary>
    /// Sound tables the hero's components hold: the hero plays these from code (Beast crest rage) on pooled
    /// sources that are neither its children nor run by its FSMs.
    /// </summary>
    public bool IsHeroTable(RandomAudioClipTable table)
    {
        if (GameRefs.Hero is not { } hero)
        {
            return false;
        }
        if (_tablesOf != hero)
        {
            _tablesOf = hero;
            _heroTables.Clear();
            foreach (MonoBehaviour behaviour in hero.GetComponentsInChildren<MonoBehaviour>(true))
            {
                CollectTables(behaviour);
            }
        }
        return _heroTables.Contains(table);
    }

    private void CollectTables(MonoBehaviour behaviour)
    {
        if (!behaviour)
        {
            return;
        }
        const BindingFlags Fields = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        foreach (FieldInfo field in behaviour.GetType().GetFields(Fields))
        {
            switch (field.GetValue(behaviour))
            {
                case RandomAudioClipTable table when table:
                    _heroTables.Add(table);
                    break;
                case RandomAudioClipTable[] tables:
                    _heroTables.UnionWith(tables.Where(table => table));
                    break;
            }
        }
    }

    private bool IsOwn(AudioSource source)
    {
        // Thrown tools play their own sounds on the partner's copy.
        bool ownTool = ToolReplicas.IsOwnTool(source.gameObject)
            || ToolReplicas.IsOwnTool(FsmExecutionStack.ExecutingFsm?.GameObject);
        if (ownTool || GameRefs.Hero is not { } hero || IsMovement(hero, source))
        {
            return false;
        }
        if (HeroTablePlaying || source.transform.IsChildOf(hero.transform) || IsTool(source.gameObject))
        {
            return true;
        }
        GameObject? running = FsmExecutionStack.ExecutingFsm?.GameObject;
        return running && (running!.transform.IsChildOf(hero.transform) || IsTool(running));
    }

    /// <summary>Movement sounds already travel through <see cref="HeroAudioSync"/>.</summary>
    private bool IsMovement(HeroController hero, AudioSource source)
    {
        if (_movementOwner != hero)
        {
            _movementOwner = hero;
            _movementSources.Clear();
            HeroAudioController audio = hero.AudioCtrl;
            foreach (HeroSounds sound in System.Enum.GetValues(typeof(HeroSounds)))
            {
                if (HeroAudioSync.SourceOf(audio, sound) is { } movement)
                {
                    _movementSources.Add(movement);
                }
            }
            _movementSources.Add(audio.footSteps);
        }
        return _movementSources.Contains(source);
    }

    /// <summary>Part of a thrown tool or deployable: something near it deals damage as a tool.</summary>
    private bool IsTool(GameObject go)
    {
        int key = go.GetInstanceID();
        if (!_toolObjects.TryGetValue(key, out bool tool))
        {
            DamageEnemies? above = go.GetComponentInParent<DamageEnemies>(true);
            DamageEnemies? below = go.GetComponentInChildren<DamageEnemies>(true);
            tool = (above && above!.representingTool) || (below && below!.representingTool);
            _toolObjects[key] = tool;
        }
        return tool;
    }

    private void OnClip(PacketReader reader)
    {
        int name = reader.Int();
        Vector2 at = new(reader.Float(), reader.Float());
        float volume = reader.Float();
        float pitch = reader.Float();
        if (!Coop.Rooms.PartnerHere || Find(name) is not { } clip)
        {
            return;
        }
        AudioSource voice = NextVoice();
        voice.pitch = pitch;
        voice.PlayOneShot(clip, volume * HeroAudioSync.VolumeAt(at));
    }

    private AudioClip? Find(int name)
    {
        if (_clips.TryGetValue(name, out AudioClip? clip) && clip)
        {
            return clip;
        }
        if (Time.unscaledTime - _lastScan < RescanCooldown)
        {
            return null;
        }
        _lastScan = Time.unscaledTime;
        foreach (AudioClip loaded in Resources.FindObjectsOfTypeAll<AudioClip>())
        {
            _clips[Fnv.Hash(loaded.name)] = loaded;
        }
        return _clips.TryGetValue(name, out clip) && clip ? clip : null;
    }

    private AudioSource NextVoice()
    {
        if (!_speaker)
        {
            _speaker = new GameObject("SilksongCoop Partner Clips");
            Object.DontDestroyOnLoad(_speaker);
            _voices.Clear();
            for (int i = 0; i < Voices; i++)
            {
                AudioSource voice = _speaker.AddComponent<AudioSource>();
                voice.playOnAwake = false;
                voice.spatialBlend = 0f;
                _voices.Add(voice);
            }
        }
        AudioSource next = _voices[_nextVoice];
        _nextVoice = (_nextVoice + 1) % Voices;
        // Same mixer bus as the player's own effects, so volume settings apply.
        next.outputAudioMixerGroup = GameRefs.Hero?.AudioCtrl.jump.outputAudioMixerGroup;
        return next;
    }
}
