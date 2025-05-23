using System.Collections.Immutable;
using FrooxEngine;
using HarmonyLib;
using Restrainite.Enums;

namespace Restrainite.Patches;

[HarmonyPatch]
internal static class PreventHearing
{
    private static readonly SlotTagPermissionChecker SlotTagPermissionChecker = new(
        PreventionType.AllowHearingBySlotTags, PreventionType.DenyHearingBySlotTags);

    internal static void Initialize()
    {
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
            preventionType != PreventionType.HearingVolume)
            return;
        MarkAudioOutputsDirty();
    }

    private static void MarkAudioOutputsDirty()
    {
        var slot = Engine.Current?.WorldManager?.FocusedWorld.RootSlot;
        slot?.RunInUpdates(0, () =>
        {
            var list = slot.GetComponentsInChildren<AudioOutput>();
            if (list == null) return;
            foreach (var audioOutput in list) audioOutput?.MarkChangeDirty();
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
             preventionType != PreventionType.DenyHearingBySlotTags) ||
            !RestrainiteMod.IsRestricted(preventionType))
            return;
        MarkAudioOutputsDirty();
    }

    /*
     * PreventHearing OFF EnforceSelectiveHearing OFF: No mute override
     * PreventHearing OFF EnforceSelectiveHearing ON: Anyone not in ESH list is muted
     * PreventHearing ON EnforceSelectiveHearing OFF: Everyone muted
     * PreventHearing ON EnforceSelectiveHearing ON: Everyone muted
     */
    [HarmonyPostfix]
    [HarmonyPatch(typeof(AudioOutput), nameof(AudioOutput.ActualVolume), MethodType.Getter)]
    private static float AudioOutput_ActualVolume_Getter_Postfix(float result, AudioOutput __instance)
    {
        if (RestrainiteMod.IsRestricted(PreventionType.PreventHearing)) return 0.0f;
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
        if (RestrainiteMod.IsRestricted(PreventionType.EnforceSelectiveHearing) &&
            !RestrainiteMod.GetStringSet(PreventionType.EnforceSelectiveHearing).Contains(userId)) return 0.0f;
        return RestrainiteMod.IsRestricted(PreventionType.PreventHearingOfUsers) ? 0.0f : volume;
    }

    private static bool ShouldHearSounds(Slot? slot)
    {
        return !RestrainiteMod.IsRestricted(PreventionType.PreventHearingOfSounds) &&
               SlotTagPermissionChecker.IsAllowed(slot);
    }
}