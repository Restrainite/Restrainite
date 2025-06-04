using System;
using Elements.Assets;
using FrooxEngine;
using HarmonyLib;
using Restrainite.Enums;

namespace Restrainite.Patches;

[HarmonyPatch]
internal static class SpeakingVolume
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(UserAudioStream<MonoSample>), "OnNewAudioData")]
    private static void UserAudioStream_OnNewAudioData_Prefix(ref Span<StereoSample> buffer)
    {
        if (!RestrainiteMod.IsRestricted(PreventionType.SpeakingVolume)) return;
        var multiplier = RestrainiteMod.GetLowestFloat(PreventionType.SpeakingVolume);
        if (float.IsNaN(multiplier)) return;
        if (multiplier < 0.0f) multiplier = 0.0f;
        if (multiplier > 1.0f) multiplier = 1.0f;
        var newBuffer = new Span<StereoSample>(buffer.ToArray());
        for (var i = 0; i < newBuffer.Length; i++) newBuffer[i] *= multiplier;
        buffer = newBuffer;
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(AudioDeviceVolume), "OnNewNormalizedSamples")]
    private static void AudioDeviceVolume_OnNewNormalizedSamples_Prefix(ref Span<StereoSample> buffer)
    {
        if (!RestrainiteMod.IsRestricted(PreventionType.SpeakingVolume)) return;
        var multiplier = RestrainiteMod.GetLowestFloat(PreventionType.SpeakingVolume);
        if (float.IsNaN(multiplier)) return;
        if (multiplier < 0.0f) multiplier = 0.0f;
        if (multiplier > 1.0f) multiplier = 1.0f;
        var newBuffer = new Span<StereoSample>(buffer.ToArray());
        for (var i = 0; i < newBuffer.Length; i++) newBuffer[i] *= multiplier;
        buffer = newBuffer;
    }
}