using System.Reflection;
using FrooxEngine;
using HarmonyLib;
using ResoniteModLoader;
using Restrainite.RestrictionTypes.Base;

namespace Restrainite.Patches;

[HarmonyPatch]
internal static class PreventHearing
{
    private static readonly SlotTagPermissionChecker SlotTagPermissionChecker = new(
        Restrictions.AllowHearingBySlotTags,
        Restrictions.DenyHearingBySlotTags);

    private static readonly FieldInfo? AudioManagerOutputs = AccessTools.Field(typeof(AudioManager), "_outputs");
    private static int _alreadyMarkedForNextUpdate;

    internal static void Initialize()
    {
        if (AudioManagerOutputs == null)
        {
            ResoniteMod.Error(RestrainiteMod.LogReportUrl + " Failed to find method AudioOutput._outputs");
            RestrainiteMod.SuccessfullyPatched = false;
        }

        Restrictions.AllowHearingBySlotTags.OnChanged += MarkAudioOutputsDirty;
        Restrictions.DenyHearingBySlotTags.OnChanged += MarkAudioOutputsDirty;
        Restrictions.PreventHearing.OnChanged += MarkAudioOutputsDirty;
        Restrictions.PreventHearingOfUsers.OnChanged += MarkAudioOutputsDirty;
        Restrictions.PreventHearingOfSounds.OnChanged += MarkAudioOutputsDirty;
        Restrictions.EnforceSelectiveHearing.OnChanged += MarkAudioOutputsDirty;
        Restrictions.HearingVolume.OnChanged += MarkAudioOutputsDirty;
        Restrictions.AlwaysHearSelectedUsers.OnChanged += MarkAudioOutputsDirty;
    }

    internal static void MarkAudioOutputsDirty(IRestriction restriction)
    {
        var tick = Engine.Current.UpdateTick;
        if (Interlocked.Exchange(ref _alreadyMarkedForNextUpdate, tick) == tick) return;
        var world = Engine.Current?.WorldManager?.FocusedWorld;
        world?.RunInUpdates(1, () =>
        {
            var sources = AudioManagerOutputs?.GetValue(world.Audio);
            if (sources is not HashSet<AudioOutput> audioOutputs) return;
            foreach (var audioOutput in audioOutputs.Where(audioOutput => audioOutput.IsRegistered))
                audioOutput.MarkChangeDirty();
        });
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(AudioOutput), nameof(AudioOutput.ActualVolume), MethodType.Getter)]
    private static float AudioOutput_ActualVolume_Getter_Postfix(float result, AudioOutput __instance)
    {
        var volume = result;
        if (Restrictions.HearingVolume.IsRestricted)
        {
            var volumeMultiplier = Restrictions.HearingVolume.LowestFloat.Value;
            if (!float.IsNaN(volumeMultiplier))
            {
                volume = volumeMultiplier * volume;
            }
        }

        var slot = __instance.Slot;
        var activeUserId = slot?.ActiveUser?.UserID;
        if (activeUserId is null) return ShouldHearSounds(slot) ? volume : 0.0f;
        if (Restrictions.AlwaysHearSelectedUsers.IsRestricted &&
            Restrictions.AlwaysHearSelectedUsers.StringSet.Contains(activeUserId)) return result;
        if (Restrictions.EnforceSelectiveHearing.IsRestricted &&
            !Restrictions.EnforceSelectiveHearing.StringSet.Contains(activeUserId)) return 0.0f;
        return Restrictions.PreventHearing.IsRestricted ||
               Restrictions.PreventHearingOfUsers.IsRestricted
            ? 0.0f
            : volume;
    }

    private static bool ShouldHearSounds(Slot? slot)
    {
        return !Restrictions.PreventHearing.IsRestricted &&
               !Restrictions.PreventHearingOfSounds.IsRestricted &&
               SlotTagPermissionChecker.IsAllowed(slot);
    }
}