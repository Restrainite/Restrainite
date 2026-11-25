using Elements.Assets;
using FrooxEngine;
using HarmonyLib;

namespace Restrainite.Patches;

[HarmonyPatch]
internal static class SpeakingVolume
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(UserAudioStream<MonoSample>), "OnNewAudioData")]
    private static void UserAudioStream_OnNewAudioData_Prefix(ref ReadOnlySpan<StereoSample> buffer)
    {
        ScaleSamples(ref buffer);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(AudioDeviceVolume), "OnNewNormalizedSamples")]
    private static void AudioDeviceVolume_OnNewNormalizedSamples_Prefix(ref ReadOnlySpan<StereoSample> buffer)
    {
        ScaleSamples(ref buffer);
    }

    private static void ScaleSamples(ref ReadOnlySpan<StereoSample> buffer)
    {
        if (!Restrictions.SpeakingVolume.IsRestricted) return;
        var multiplier = Restrictions.SpeakingVolume.LowestFloat.Value;
        if (float.IsNaN(multiplier)) return;

        var source = buffer;
        var scaledSamples = GC.AllocateUninitializedArray<StereoSample>(source.Length);
        for (var i = 0; i < scaledSamples.Length; i++) scaledSamples[i] = source[i] * multiplier;
        buffer = scaledSamples;
    }
}