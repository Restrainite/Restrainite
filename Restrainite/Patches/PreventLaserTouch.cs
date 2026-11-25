using System.Reflection;
using System.Runtime.CompilerServices;
using FrooxEngine;
using HarmonyLib;
using Renderite.Shared;
using ResoniteModLoader;
using Restrainite.RestrictionTypes.Base;

namespace Restrainite.Patches;

[HarmonyPatch]
internal static class PreventLaserTouch
{
    private static readonly ConditionalWeakTable<InteractionHandler, OriginalState> DisabledLasers = new();

    private static readonly FieldInfo?
        LaserEnabledField = AccessTools.Field(typeof(InteractionHandler), "_laserEnabled");

    private static readonly MethodInfo?
        ClearLaserTimeoutMethod = AccessTools.Method(typeof(InteractionLaser), "ClearLaserTimeout");


    internal static void Initialize()
    {
        if (LaserEnabledField == null)
        {
            ResoniteMod.Error(RestrainiteMod.LogReportUrl + " Failed to find field InteractionHandler._laserEnabled");
            RestrainiteMod.SuccessfullyPatched = false;
        }

        if (ClearLaserTimeoutMethod == null)
        {
            ResoniteMod.Error(RestrainiteMod.LogReportUrl +
                              " Failed to find method InteractionLaser.ClearLaserTimeout");
            RestrainiteMod.SuccessfullyPatched = false;
        }

        Restrictions.PreventLaserTouch.OnChanged += OnRestrictionChanged;
    }

    private static void OnRestrictionChanged(IRestriction restriction)
    {
        // Restore the lasers in all worlds, not only in the focused one
        foreach (var (interactionHandler, _) in DisabledLasers) ScheduleUpdate(interactionHandler);

        var user = Engine.Current?.WorldManager?.FocusedWorld?.LocalUser;
        if (user is null) return;
        ScheduleUpdate(user.GetInteractionHandler(Chirality.Left));
        ScheduleUpdate(user.GetInteractionHandler(Chirality.Right));
    }

    private static void ScheduleUpdate(InteractionHandler? interactionHandler)
    {
        if (interactionHandler is not { IsRemoved: false, World.IsDestroyed: false }) return;
        interactionHandler.RunInUpdates(0, () => UpdateLaser(interactionHandler));
    }

    private static void UpdateLaser(InteractionHandler interactionHandler)
    {
        if (interactionHandler.IsRemoved) return;
        if (LaserEnabledField?.GetValue(interactionHandler) is not Sync<bool> laserEnabled) return;

        if (IsRestricted(interactionHandler))
        {
            DisabledLasers.TryAdd(interactionHandler, new OriginalState(laserEnabled.Value));
            laserEnabled.Value = false;
            if (interactionHandler.Laser != null) ClearLaserTimeoutMethod?.Invoke(interactionHandler.Laser, []);
        }
        else if (DisabledLasers.TryGetValue(interactionHandler, out var originalState))
        {
            DisabledLasers.Remove(interactionHandler);
            laserEnabled.Value = originalState.LaserEnabled;
        }
    }

    private static bool IsRestricted(InteractionHandler interactionHandler)
    {
        return Restrictions.PreventLaserTouch.IsRestricted &&
               Restrictions.PreventLaserTouch.Chirality.IsRestricted(interactionHandler.Side.Value) &&
               interactionHandler.World != Userspace.UserspaceWorld &&
               interactionHandler.IsUnderLocalUser;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(InteractionHandler), "OnStart")]
    private static void InteractionHandler_OnStart_Postfix(InteractionHandler __instance)
    {
        if (Restrictions.PreventLaserTouch.IsRestricted) ScheduleUpdate(__instance);
    }

    private sealed record OriginalState(bool LaserEnabled);
}