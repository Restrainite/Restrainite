using System.Reflection;
using Awwdio;
using Elements.Core;
using FrooxEngine;
using HarmonyLib;
using ResoniteModLoader;
using Restrainite.Enums;
using AudioOutput = FrooxEngine.AudioOutput;

namespace Restrainite.Patches;

[HarmonyPatch]
internal static class MaximumHearingDistance
{
    private static MethodInfo? _nativeOutputMethod;
    private static MethodInfo? _audioInletMethod;

    internal static void Initialize()
    {
        RestrainiteMod.OnRestrictionChanged += OnChange;

        _nativeOutputMethod = AccessTools.DeclaredPropertyGetter(typeof(AudioOutput), "NativeOutput");
        if (_nativeOutputMethod == null) ResoniteMod.Warn("Failed to find method AudioOutput.NativeOutput");

        _audioInletMethod = AccessTools.DeclaredPropertyGetter(typeof(AudioManager), "EffectsInlet");
        if (_audioInletMethod == null) ResoniteMod.Warn("Failed to find method AudioManager.EffectsInlet");
    }

    private static void OnChange(PreventionType preventionType, bool value)
    {
        if (preventionType != PreventionType.MaximumHearingDistance)
            return;
        var list = Engine.Current?.WorldManager?.FocusedWorld?.RootSlot?.GetComponentsInChildren<AudioOutput>();
        if (list == null) return;
        foreach (var audioOutput in list) audioOutput?.MarkChangeDirty();
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(AudioOutput), "UpdateNativeOutput")]
    private static bool AudioOutput_UpdateNativeOutput_Prefix(
        AudioOutput __instance,
        ChangesBatch batch,
        ref bool ____updateRegistered,
        ref IAudioShape ____audioShape)
    {
        if (!RestrainiteMod.IsRestricted(PreventionType.MaximumHearingDistance)) return true;
        if (_nativeOutputMethod == null || _audioInletMethod == null) return true;

        var restrictedDistance = RestrainiteMod.GetLowestFloat(PreventionType.MaximumHearingDistance);
        if (float.IsNaN(restrictedDistance)) return true;
        if (restrictedDistance <= 0.0f) restrictedDistance = 0.0f;

        if (__instance.Global.Value ?? MathX.Approximately(__instance.SpatialBlend.Value, 0.0f)) return true;

        ____updateRegistered = false;

        var nativeOutput = (Awwdio.AudioOutput?)_nativeOutputMethod.Invoke(__instance, []);
        if (nativeOutput == null || __instance.IsRemoved)
            return false;

        __instance.GetActualDistances(out var minDistance,
            out var maxDistance,
            out var spatializationStartDistance,
            out var spatializationTransitionRange);

        if (restrictedDistance < maxDistance) maxDistance = restrictedDistance;
        if (minDistance > maxDistance)
        {
            minDistance = maxDistance;
            maxDistance += minDistance * 0.0001f;
        }

        if (____audioShape is not SphereAudioShape sphereAudioShape)
            ____audioShape = sphereAudioShape = new SphereAudioShape(nativeOutput);
        sphereAudioShape.Update(batch, minDistance, maxDistance,
            __instance.RolloffMode.Value == AudioRolloffCurve.LogarithmicInfinite
                ? AudioRolloffCurve.LogarithmicFadeOff
                : __instance.RolloffMode.Value);

        nativeOutput.Update(batch, __instance.Slot.GlobalRigidTransform, __instance.ActualVolume,
            __instance.Priority.Value, __instance.Spatialize,
            MathX.Clamp01(__instance.SpatialBlend.Value),
            spatializationStartDistance, spatializationTransitionRange,
            MathX.Max(0.0f, MathX.FilterInvalid(__instance.Pitch.Value)),
            MathX.Max(0.0f, MathX.FilterInvalid(__instance.DopplerLevel.Value)));


        nativeOutput.Update(batch, __instance.Source.Target, ____audioShape,
            __instance.IgnoreAudioEffects.Value ? null : (AudioInlet)_audioInletMethod.Invoke(__instance.Audio, []));
        return false;
    }
}