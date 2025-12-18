using System;
using Elements.Assets;
using FrooxEngine;
using HarmonyLib;

namespace Restrainite.Patches;

[HarmonyPatch]
internal static class SpeakingVolume
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(UserAudioStream<MonoSample>), "OnNewAudioData")]
    private static void UserAudioStream_OnNewAudioData_Prefix(ref Span<StereoSample> buffer)
    {
        if (!Restrictions.SpeakingVolume.IsRestricted) return;
        var multiplier = Restrictions.SpeakingVolume.LowestFloat.Value;
        if (float.IsNaN(multiplier)) return;
        var newBuffer = new Span<StereoSample>(buffer.ToArray());
        for (var i = 0; i < newBuffer.Length; i++) newBuffer[i] *= multiplier;
        buffer = newBuffer;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(AudioDeviceVolume), "OnNewNormalizedSamples")]
    private static void AudioDeviceVolume_OnNewNormalizedSamples_Prefix(ref Span<StereoSample> buffer)
    {
        if (!Restrictions.SpeakingVolume.IsRestricted) return;
        var multiplier = Restrictions.SpeakingVolume.LowestFloat.Value;
        if (float.IsNaN(multiplier)) return;
        var newBuffer = new Span<StereoSample>(buffer.ToArray());
        for (var i = 0; i < newBuffer.Length; i++) newBuffer[i] *= multiplier;
        buffer = newBuffer;
    }
}