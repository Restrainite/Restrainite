using FrooxEngine;
using HarmonyLib;
using Restrainite.Enums;

namespace Restrainite.Patches;

[HarmonyPatch]
internal static class PreventHearing
{
    internal static void Initialize()
    {
        RestrainiteMod.OnRestrictionChanged += OnChange;
    }

    private static void OnChange(PreventionType preventionType, bool value)
    {
        if (preventionType != PreventionType.PreventHearing &&
            preventionType != PreventionType.PreventHearingOfUsers &&
            preventionType != PreventionType.EnforceSelectiveHearing)
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
        var activeUser = __instance.Slot?.ActiveUser;
        if (activeUser == null) return RestrainiteMod.IsRestricted(PreventionType.PreventHearing) ? 0.0f : result;
        var userId = activeUser.UserID;
        if (userId is null) return RestrainiteMod.IsRestricted(PreventionType.PreventHearing) ? 0.0f : result;
        if (RestrainiteMod.IsRestricted(PreventionType.EnforceSelectiveHearing) &&
            !RestrainiteMod.GetStrings(PreventionType.EnforceSelectiveHearing).Contains(userId)) return 0.0f;
        return RestrainiteMod.IsRestricted(PreventionType.PreventHearingOfUsers) ? 0.0f : result;
    }
}