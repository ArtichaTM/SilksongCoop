using GlobalEnums;
using HarmonyLib;
using SilksongCoop.Game;
using UnityEngine;

namespace SilksongCoop.Patches;

/// <summary>Reports the local Hornet's movement sounds to the partner (D36).</summary>
[HarmonyPatch(typeof(HeroAudioController))]
internal static class AudioPatches
{
    private static HeroAudioSync? Sync(HeroAudioController audio)
    {
        bool local = GameRefs.Hero is { } hero && hero.AudioCtrl == audio;
        return local && Coop.Current is { InCoop: true } coop ? coop.HeroAudio : null;
    }

    private static bool IsPlaying(HeroAudioController audio, HeroSounds sound)
    {
        AudioSource? source = HeroAudioSync.SourceOf(audio, sound);
        return source && source!.isPlaying;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(HeroAudioController.PlaySound))]
    private static void PlayBefore(
        HeroAudioController __instance,
        HeroSounds soundEffect,
        out bool __state
    ) => __state = IsPlaying(__instance, soundEffect);

    [HarmonyPostfix]
    [HarmonyPatch(nameof(HeroAudioController.PlaySound))]
    private static void PlayAfter(HeroAudioController __instance, HeroSounds soundEffect, bool __state)
    {
        // A loop asked to play while already playing doesn't restart; one-shots always do.
        bool started = IsPlaying(__instance, soundEffect) && !(__state && HeroAudioSync.IsLoop(soundEffect));
        if (started)
        {
            Sync(__instance)?.SendPlay(soundEffect);
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(HeroAudioController.StopSound))]
    private static void Stop(HeroAudioController __instance, HeroSounds soundEffect)
    {
        // The hero stops footsteps and loops every frame; only a real stop is worth a message.
        if (HeroAudioSync.IsLoop(soundEffect) && IsPlaying(__instance, soundEffect))
        {
            Sync(__instance)?.SendStop(soundEffect);
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(HeroAudioController.StopAllSounds))]
    private static void StopAll(HeroAudioController __instance) => Sync(__instance)?.SendStopAll();

    [HarmonyPrefix]
    [HarmonyPatch(nameof(HeroAudioController.PlayFootstep))]
    private static void Footstep(HeroAudioController __instance)
    {
        if (__instance.CanPlayFootsteps && !__instance.BlockFootstepAudio)
        {
            Sync(__instance)?.SendFootstep();
        }
    }
}

/// <summary>Every <c>Play</c> and <c>PlayOneShot</c> in the game funnels through these two helpers.</summary>
[HarmonyPatch(typeof(AudioSource))]
internal static class ClipPatches
{
    private static ClipAudioSync? Sync => Coop.Current is { InCoop: true } coop ? coop.ClipAudio : null;

    [HarmonyPrefix]
    [HarmonyPatch("PlayHelper")]
    private static void Play(AudioSource source) => Sync?.OnPlayed(source, source.clip, 1f);

    [HarmonyPrefix]
    [HarmonyPatch("PlayOneShotHelper")]
    private static void PlayOneShot(AudioSource source, AudioClip clip, float volumeScale) =>
        Sync?.OnPlayed(source, clip, volumeScale);
}
