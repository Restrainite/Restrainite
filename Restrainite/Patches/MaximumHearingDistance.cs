using System.Reflection;
using Awwdio;
using Elements.Core;
using FrooxEngine;
using HarmonyLib;
using ResoniteModLoader;
using Restrainite.RestrictionTypes.Base;
using AudioOutput = FrooxEngine.AudioOutput;

namespace Restrainite.Patches;

[HarmonyPatch]
internal static class MaximumHearingDistance
{
    private static readonly MethodInfo? NativeOutputMethod =
        AccessTools.PropertyGetter(typeof(AudioOutput), "NativeOutput");

    private static readonly MethodInfo? AudioInletMethod =
        AccessTools.PropertyGetter(typeof(AudioManager), "EffectsInlet");

    internal static void Initialize()
    {
        if (NativeOutputMethod == null)
        {
            ResoniteMod.Error(RestrainiteMod.LogReportUrl + " Failed to find method AudioOutput.NativeOutput");
            RestrainiteMod.SuccessfullyPatched = false;
            return;
        }

        if (AudioInletMethod == null)
        {
            ResoniteMod.Error(RestrainiteMod.LogReportUrl + " Failed to find method AudioManager.EffectsInlet");
            RestrainiteMod.SuccessfullyPatched = false;
            return;
        }

        Restrictions.MaximumHearingDistance.OnChanged += OnChanged;
        Restrictions.AlwaysHearSelectedUsers.OnChanged += OnChanged;
    }

    private static void OnChanged(IRestriction restriction)
    {
        PreventHearing.MarkAudioOutputsDirty(restriction);
    }


    [HarmonyPrefix]
    [HarmonyPatch(typeof(AudioOutput), "UpdateNativeOutput")]
    private static bool AudioOutput_UpdateNativeOutput_Prefix(
        AudioOutput __instance,
        ChangesBatch batch,
        ref bool ____updateRegistered,
        ref IAudioShape ____audioShape)
    {
        if (!Restrictions.MaximumHearingDistance.IsRestricted) return true;
        if (NativeOutputMethod == null || AudioInletMethod == null) return true;
        if (IsAlwaysHearingSelectedUsers(__instance)) return true;

        var restrictedDistance = Restrictions.MaximumHearingDistance.LowestFloat.Value;
        if (float.IsNaN(restrictedDistance)) return true;
        if (restrictedDistance <= 0.0f) restrictedDistance = 0.0f;

        if (__instance.Global.Value ?? MathX.Approximately(__instance.SpatialBlend.Value, 0.0f)) return true;

        ____updateRegistered = false;

        var nativeOutput = (Awwdio.AudioOutput?)NativeOutputMethod.Invoke(__instance, []);
        if (nativeOutput == null || __instance.IsRemoved)
            return false;

        __instance.GetActualDistances(out var minDistance,
            out var maxDistance,
            out var spatializationStartDistance,
            out var spatializationTransitionRange);

        if (restrictedDistance < maxDistance)
        {
            maxDistance = restrictedDistance;
            if (minDistance > maxDistance) minDistance = maxDistance * 0.99f;
        }

        if (____audioShape is not SphereAudioShape sphereAudioShape)
            ____audioShape = sphereAudioShape = new SphereAudioShape(nativeOutput);
        sphereAudioShape.Update(batch, minDistance, maxDistance,
            __instance.RolloffMode.Value is AudioRolloffCurve.LogarithmicInfinite
                or AudioRolloffCurve.LogarithmicClamped
                ? AudioRolloffCurve.LogarithmicFadeOff
                : __instance.RolloffMode.Value);

        nativeOutput.Update(batch, __instance.Slot.GlobalRigidTransform, __instance.ActualVolume,
            __instance.Priority.Value, __instance.Spatialize,
            MathX.Clamp01(__instance.SpatialBlend.Value),
            spatializationStartDistance, spatializationTransitionRange,
            MathX.Max(0.0f, MathX.FilterInvalid(__instance.Pitch.Value)),
            MathX.Max(0.0f, MathX.FilterInvalid(__instance.DopplerLevel.Value)));


        nativeOutput.Update(batch, __instance.Source.Target, ____audioShape,
            __instance.IgnoreAudioEffects.Value ? null : (AudioInlet)AudioInletMethod.Invoke(__instance.Audio, []));
        return false;
    }

    private static bool IsAlwaysHearingSelectedUsers(AudioOutput audioOutput)
    {
        var slot = audioOutput.Slot;
        var activeUser = slot?.ActiveUser;

        if (activeUser == null || audioOutput.AudioTypeGroup.Value != AudioTypeGroup.Voice)
            return false;
        var userId = activeUser.UserID;
        if (userId is null) return false;
        return Restrictions.AlwaysHearSelectedUsers.IsRestricted &&
               Restrictions.AlwaysHearSelectedUsers.SetContains(userId);
    }
}