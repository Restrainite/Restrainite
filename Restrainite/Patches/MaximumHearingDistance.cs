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
    private static bool _hasShownWarning;

    internal static void Initialize()
    {
        RestrainiteMod.OnRestrictionChanged += OnChange;
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
    private static bool AudioOutput_UpdateNativeOutput_Prefix(AudioOutput __instance, ChangesBatch batch,
        ref bool ____updateRegistered, ref IAudioShape ____audioShape)
    {
        if (!RestrainiteMod.IsRestricted(PreventionType.MaximumHearingDistance)) return true;

        var restrictedDistance = RestrainiteMod.GetLowestFloat(PreventionType.MaximumHearingDistance);
        if (float.IsNaN(restrictedDistance) || restrictedDistance < 0) return true;

        if (__instance.Global.Value ?? MathX.Approximately(__instance.SpatialBlend.Value, 0.0f)) return true;

        var nativeOutputMethod = AccessTools.DeclaredPropertyGetter(typeof(AudioOutput), "NativeOutput");
        var audioInletMethod = AccessTools.DeclaredPropertyGetter(typeof(AudioManager), "EffectsInlet");
        if (nativeOutputMethod == null || audioInletMethod == null)
        {
            if (_hasShownWarning) return true;
            _hasShownWarning = true;
            ResoniteMod.Warn($"Failed to find methods {nativeOutputMethod} and {audioInletMethod}");
            return true;
        }

        var nativeOutput = (Awwdio.AudioOutput?)nativeOutputMethod.Invoke(__instance, []);

        ____updateRegistered = false;
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
            __instance.IgnoreAudioEffects.Value ? null : (AudioInlet)audioInletMethod.Invoke(__instance.Audio, []));
        return false;
    }
}