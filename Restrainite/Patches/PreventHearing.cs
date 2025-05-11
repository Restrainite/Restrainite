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
    }

    private static void OnChange(PreventionType preventionType, bool value)
    {
        if (preventionType != PreventionType.PreventHearing &&
            preventionType != PreventionType.PreventHearingOfUsers &&
            preventionType != PreventionType.EnforceSelectiveHearing &&
            preventionType != PreventionType.AllowHearingBySlotTags &&
            preventionType != PreventionType.DenyHearingBySlotTags &&
            preventionType != PreventionType.HearingVolumeMultiplier)
            return;
        var list = Engine.Current?.WorldManager?.FocusedWorld?.RootSlot?.GetComponentsInChildren<AudioOutput>();
        if (list == null) return;
        foreach (var audioOutput in list) audioOutput?.MarkChangeDirty();
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
        var slot = __instance.Slot;
        var activeUser = slot?.ActiveUser;
        var volume = result;
        if (RestrainiteMod.IsRestricted(PreventionType.HearingVolumeMultiplier))
        {
            var volumeMultiplier = RestrainiteMod.GetLowestFloat(PreventionType.HearingVolumeMultiplier);
            if (!float.IsNaN(volumeMultiplier))
            {
                if (volumeMultiplier <= 0.0f) volumeMultiplier = 0.0f;
                if (volumeMultiplier >= 1.0f) volumeMultiplier = 1.0f;
                volume = volumeMultiplier * volume;
            }
        }

        if (activeUser == null) return ShouldHear(slot) ? volume : 0.0f;
        var userId = activeUser.UserID;
        if (userId is null) return ShouldHear(slot) ? volume : 0.0f;
        if (RestrainiteMod.IsRestricted(PreventionType.EnforceSelectiveHearing) &&
            !RestrainiteMod.GetStrings(PreventionType.EnforceSelectiveHearing).Contains(userId)) return 0.0f;
        return RestrainiteMod.IsRestricted(PreventionType.PreventHearingOfUsers) ? 0.0f : volume;
    }

    private static bool ShouldHear(Slot? slot)
    {
        return !RestrainiteMod.IsRestricted(PreventionType.PreventHearing) && SlotTagPermissionChecker.IsAllowed(slot);
    }
}