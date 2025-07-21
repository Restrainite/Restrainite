using System.Reflection;
using FrooxEngine;
using HarmonyLib;
using ResoniteModLoader;
using Restrainite.RestrictionTypes.Base;

namespace Restrainite.Patches;

internal static class PreventLaserTouch
{
    private static bool _leftOriginalValue = true;
    private static bool _leftIsRestricted;
    private static bool _rightOriginalValue = true;
    private static bool _rightIsRestricted;

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
        var user = Engine.Current?.WorldManager?.FocusedWorld?.LocalUser;
        if (user is null) return;

        var leftInteractionHandler = user.GetInteractionHandler(Chirality.Left);
        leftInteractionHandler.RunInUpdates(0, () =>
            SetLaserActive(leftInteractionHandler, ref _leftOriginalValue, ref _leftIsRestricted, Chirality.Left));

        var rightInteractionHandler = user.GetInteractionHandler(Chirality.Right);
        rightInteractionHandler.RunInUpdates(0, () =>
            SetLaserActive(rightInteractionHandler, ref _rightOriginalValue, ref _rightIsRestricted, Chirality.Right));
    }

    private static void SetLaserActive(InteractionHandler? interactionHandler, ref bool originalValue,
        ref bool isRestricted, Chirality chirality)
    {
        if (interactionHandler == null) return;
        if (LaserEnabledField?.GetValue(interactionHandler) is not Sync<bool> syncBool) return;
        if (Restrictions.PreventLaserTouch.IsRestricted &&
            Restrictions.PreventLaserTouch.Chirality.IsRestricted(chirality))
        {
            if (!isRestricted)
            {
                originalValue = syncBool.Value;
                isRestricted = true;
            }

            syncBool.Value = false;
            if (interactionHandler.Laser != null) ClearLaserTimeoutMethod?.Invoke(interactionHandler.Laser, []);
        }
        else if (isRestricted)
        {
            syncBool.Value = originalValue;
            isRestricted = false;
        }
    }
}