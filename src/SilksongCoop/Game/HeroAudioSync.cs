using System.Collections.Generic;
using GlobalEnums;
using SilksongCoop.Net;
using UnityEngine;

namespace SilksongCoop.Game;

/// <summary>
/// The partner's movement sounds (D36): whatever the local Hornet's <c>HeroAudioController</c> plays, stops
/// or steps is sent, and the partner's puppet plays its own copy of that sound where it stands, quieter
/// the farther it is from the listener's Hornet.
/// </summary>
internal sealed class HeroAudioSync(Coop coop) : Module(coop)
{
    private const float FullVolumeWithin = 6f;
    private const float SilentBeyond = 28f;

    private static readonly HeroSounds[] Loops =
    [
        HeroSounds.WALLSLIDE, HeroSounds.NAIL_ART_CHARGE, HeroSounds.NAIL_ART_READY,
        HeroSounds.FALLING, HeroSounds.UPDRAFT_IDLE, HeroSounds.WINDY_IDLE,
    ];

    private readonly Dictionary<HeroSounds, (AudioSource Source, float Volume)> _voices = [];
    private (AudioSource Source, float Volume)? _steps;
    private GameObject? _speaker;

    private enum Action : byte
    {
        Play,
        Stop,
        Footstep,
        StopAll,
    }

    public static bool IsLoop(HeroSounds sound) => System.Array.IndexOf(Loops, sound) >= 0;

    /// <summary>The local Hornet's source for a sound; footstep switches have none.</summary>
    public static AudioSource? SourceOf(HeroAudioController audio, HeroSounds sound)
    {
        return sound switch
        {
            HeroSounds.SOFT_LANDING => audio.softLanding,
            HeroSounds.HARD_LANDING => audio.hardLanding,
            HeroSounds.JUMP => audio.jump,
            HeroSounds.WALLJUMP => audio.walljump,
            HeroSounds.TAKE_HIT => audio.takeHit,
            HeroSounds.DASH => audio.dash,
            HeroSounds.DASH_SILK => audio.dashSilk,
            HeroSounds.BACKDASH => audio.backDash,
            HeroSounds.WALLSLIDE => audio.wallslide,
            HeroSounds.NAIL_ART_CHARGE => audio.nailArtCharge,
            HeroSounds.NAIL_ART_READY => audio.nailArtReady,
            HeroSounds.FALLING => audio.falling,
            HeroSounds.UPDRAFT_IDLE => audio.updraftIdle,
            HeroSounds.WINDY_IDLE => audio.windyIdle,
            _ => null,
        };
    }

    public override void Register(Session session) => session.On(MessageType.HeroSound, OnSound);

    public override void Update()
    {
        if (_speaker is null || !_speaker)
        {
            return;
        }
        float falloff = Falloff();
        foreach ((AudioSource source, float volume) in _voices.Values)
        {
            if (source.isPlaying)
            {
                source.volume = volume * falloff;
            }
        }
    }

    public override void OnLeft() => Silence();

    public override void Dispose()
    {
        if (_speaker)
        {
            Object.Destroy(_speaker);
        }
    }

    public void SendPlay(HeroSounds sound) => Send(Action.Play, sound);

    public void SendStop(HeroSounds sound) => Send(Action.Stop, sound);

    public void SendFootstep() => Send(Action.Footstep, default);

    public void SendStopAll() => Send(Action.StopAll, default);

    private void Send(Action action, HeroSounds sound)
    {
        if (Coop.InCoop && Coop.Rooms.PartnerHere)
        {
            Session?.Send(MessageType.HeroSound, w => w.Byte((byte)action).Byte((byte)sound));
        }
    }

    private void OnSound(PacketReader reader)
    {
        Action action = (Action)reader.Byte();
        HeroSounds sound = (HeroSounds)reader.Byte();
        if (action == Action.StopAll || action == Action.Stop)
        {
            Stop(action == Action.StopAll ? null : sound);
            return;
        }
        if (!Prepare() || GameRefs.Hero?.AudioCtrl is not { } local)
        {
            return;
        }
        float falloff = Falloff();
        if (action == Action.Footstep)
        {
            if (_steps is { } steps && local.footstepsTable)
            {
                local.footstepsTable.PlayOneShot(steps.Source, 0f, falloff);
            }
            return;
        }
        if (_voices.TryGetValue(sound, out (AudioSource Source, float Volume) voice))
        {
            voice.Source.volume = voice.Volume * falloff;
            voice.Source.Play();
        }
    }

    private void Stop(HeroSounds? sound)
    {
        foreach (KeyValuePair<HeroSounds, (AudioSource Source, float Volume)> voice in _voices)
        {
            if (sound is null || voice.Key == sound)
            {
                voice.Value.Source.Stop();
            }
        }
    }

    private void Silence() => Stop(null);

    /// <summary>Copies the local Hornet's sources onto the puppet; false while it isn't shown.</summary>
    private bool Prepare()
    {
        Transform? partner = Coop.Heroes.PartnerTransform;
        if (partner is null || GameRefs.Hero?.AudioCtrl is not { } local)
        {
            return false;
        }
        if (_speaker && _speaker!.transform.parent == partner)
        {
            return true;
        }
        _voices.Clear();
        _speaker = new GameObject("Partner Sounds");
        _speaker.transform.SetParent(partner, false);
        foreach (HeroSounds sound in System.Enum.GetValues(typeof(HeroSounds)))
        {
            if (SourceOf(local, sound) is { } original)
            {
                _voices[sound] = (Copy(original), original.volume);
            }
        }
        _steps = (Copy(local.footSteps), local.footSteps.volume);
        return true;
    }

    private AudioSource Copy(AudioSource original)
    {
        AudioSource copy = _speaker!.AddComponent<AudioSource>();
        copy.clip = original.clip;
        copy.outputAudioMixerGroup = original.outputAudioMixerGroup;
        copy.volume = original.volume;
        copy.pitch = original.pitch;
        copy.loop = original.loop;
        copy.priority = original.priority;
        copy.spatialBlend = 0f;
        copy.playOnAwake = false;
        return copy;
    }

    /// <summary>Full volume close to the listener's Hornet, silent about a screen away.</summary>
    public static float VolumeAt(Vector2 position)
    {
        if (GameRefs.Hero is not { } hero)
        {
            return 0f;
        }
        float distance = Vector2.Distance(hero.transform.position, position);
        return Mathf.InverseLerp(SilentBeyond, FullVolumeWithin, distance);
    }

    private float Falloff()
    {
        Transform? partner = Coop.Heroes.PartnerTransform;
        return partner is null ? 0f : VolumeAt(partner.position);
    }
}
