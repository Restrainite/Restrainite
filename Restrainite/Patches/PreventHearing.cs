using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using FrooxEngine;
using HarmonyLib;
using ResoniteModLoader;
using Restrainite.Enums;

namespace Restrainite.Patches;

[HarmonyPatch]
internal static class PreventHearing
{
    private static readonly SlotTagPermissionChecker SlotTagPermissionChecker = new(
        PreventionType.AllowHearingBySlotTags, PreventionType.DenyHearingBySlotTags);

    private static readonly FieldInfo? AudioManagerOutputs = AccessTools.Field(typeof(AudioManager), "_outputs");

    internal static void Initialize()
    {
        if (AudioManagerOutputs == null)
        {
            ResoniteMod.Error(RestrainiteMod.LogReportUrl + " Failed to find method AudioOutput._outputs");
            RestrainiteMod.SuccessfullyPatched = false;
        }

        RestrainiteMod.OnRestrictionChanged += OnChange;
        RestrainiteMod.OnFloatChanged += OnChange;
        RestrainiteMod.OnStringSetChanged += OnChange;
    }

    private static void OnChange(PreventionType preventionType, bool value)
    {
        if (preventionType != PreventionType.PreventHearing &&
            preventionType != PreventionType.PreventHearingOfUsers &&
            preventionType != PreventionType.PreventHearingOfSounds &&
            preventionType != PreventionType.EnforceSelectiveHearing &&
            preventionType != PreventionType.AllowHearingBySlotTags &&
            preventionType != PreventionType.DenyHearingBySlotTags &&
            preventionType != PreventionType.HearingVolume &&
            preventionType != PreventionType.AlwaysHearSelectedUsers)
            return;
        MarkAudioOutputsDirty();
    }

    internal static void MarkAudioOutputsDirty()
    {
        var world = Engine.Current?.WorldManager?.FocusedWorld;
        world?.RunSynchronously(() =>
        {
            var sources = AudioManagerOutputs?.GetValue(world.Audio);
            if (sources is not HashSet<AudioOutput> audioOutputs) return;
            foreach (var audioOutput in audioOutputs.Where(audioOutput => audioOutput.IsRegistered))
                audioOutput.MarkChangeDirty();
        });
    }

    private static void OnChange(PreventionType preventionType, float value)
    {
        if (preventionType != PreventionType.HearingVolume || !RestrainiteMod.IsRestricted(preventionType))
            return;
        MarkAudioOutputsDirty();
    }

    private static void OnChange(PreventionType preventionType, IImmutableSet<string> stringSet)
    {
        if ((preventionType != PreventionType.EnforceSelectiveHearing &&
             preventionType != PreventionType.AllowHearingBySlotTags &&
             preventionType != PreventionType.DenyHearingBySlotTags &&
             preventionType != PreventionType.AlwaysHearSelectedUsers) ||
            !RestrainiteMod.IsRestricted(preventionType))
            return;
        MarkAudioOutputsDirty();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(AudioOutput), nameof(AudioOutput.ActualVolume), MethodType.Getter)]
    private static float AudioOutput_ActualVolume_Getter_Postfix(float result, AudioOutput __instance)
    {
        var slot = __instance.Slot;
        var activeUser = slot?.ActiveUser;
        var volume = result;
        if (RestrainiteMod.IsRestricted(PreventionType.HearingVolume))
        {
            var volumeMultiplier = RestrainiteMod.GetLowestFloat(PreventionType.HearingVolume);
            if (!float.IsNaN(volumeMultiplier))
            {
                if (volumeMultiplier <= 0.0f) volumeMultiplier = 0.0f;
                if (volumeMultiplier >= 1.0f) volumeMultiplier = 1.0f;
                volume = volumeMultiplier * volume;
            }
        }

        if (activeUser == null || __instance.AudioTypeGroup.Value != AudioTypeGroup.Voice)
            return ShouldHearSounds(slot) ? volume : 0.0f;
        var userId = activeUser.UserID;
        if (userId is null) return ShouldHearSounds(slot) ? volume : 0.0f;
        if (RestrainiteMod.IsRestricted(PreventionType.AlwaysHearSelectedUsers) &&
            RestrainiteMod.GetStringSet(PreventionType.AlwaysHearSelectedUsers).Contains(userId)) return result;
        if (RestrainiteMod.IsRestricted(PreventionType.EnforceSelectiveHearing) &&
            !RestrainiteMod.GetStringSet(PreventionType.EnforceSelectiveHearing).Contains(userId)) return 0.0f;
        return RestrainiteMod.IsRestricted(PreventionType.PreventHearing) ||
               RestrainiteMod.IsRestricted(PreventionType.PreventHearingOfUsers)
            ? 0.0f
            : volume;
    }

    private static bool ShouldHearSounds(Slot? slot)
    {
        return !RestrainiteMod.IsRestricted(PreventionType.PreventHearing) &&
               !RestrainiteMod.IsRestricted(PreventionType.PreventHearingOfSounds) &&
               SlotTagPermissionChecker.IsAllowed(slot);
    }
}