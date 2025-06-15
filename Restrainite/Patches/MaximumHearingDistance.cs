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
    private static MethodInfo? _nativeOutputMethod;
    private static MethodInfo? _audioInletMethod;

    internal static void Initialize()
    {
        Restrictions.MaximumHearingDistance.OnChanged += OnChanged;
        Restrictions.AlwaysHearSelectedUsers.OnChanged += OnChanged;

        _nativeOutputMethod = AccessTools.DeclaredPropertyGetter(typeof(AudioOutput), "NativeOutput");
        if (_nativeOutputMethod == null) ResoniteMod.Warn("Failed to find method AudioOutput.NativeOutput");

        _audioInletMethod = AccessTools.DeclaredPropertyGetter(typeof(AudioManager), "EffectsInlet");
        if (_audioInletMethod == null) ResoniteMod.Warn("Failed to find method AudioManager.EffectsInlet");
    }

    private static void OnChanged(IRestriction restriction)
    {
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
        if (!Restrictions.MaximumHearingDistance.IsRestricted) return true;
        if (_nativeOutputMethod == null || _audioInletMethod == null) return true;
        if (IsAlwaysHearingSelectedUsers(__instance)) return true;

        var restrictedDistance = Restrictions.MaximumHearingDistance.LowestFloat.Value;
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
            __instance.IgnoreAudioEffects.Value ? null : (AudioInlet)_audioInletMethod.Invoke(__instance.Audio, []));
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