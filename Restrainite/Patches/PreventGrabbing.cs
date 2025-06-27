using System.Reflection;
using FrooxEngine;
using HarmonyLib;
using ResoniteModLoader;
using Restrainite.Enums;

namespace Restrainite.Patches;

[HarmonyPatch]
internal static class PreventGrabbing
{
    private static readonly MethodInfo? EndGrabMethod =
        AccessTools.Method(typeof(InteractionHandler), "EndGrab", [typeof(bool)]);

    internal static void Initialize()
    {
        if (EndGrabMethod == null)
        {
            ResoniteMod.Error(RestrainiteMod.LogReportUrl + " Failed to find method InteractionHandler.EndGrab");
            RestrainiteMod.SuccessfullyPatched = false;
        }

        RestrainiteMod.OnRestrictionChanged += OnChange;
    }

    private static void OnChange(PreventionType preventionType, bool value)
    {
        if (preventionType != PreventionType.PreventGrabbing ||
            !value)
            return;

        if (EndGrabMethod == null) return;
        var user = Engine.Current?.WorldManager?.FocusedWorld?.LocalUser;
        if (user == null) return;
        var leftInteractionHandler = user.GetInteractionHandler(Chirality.Left);
        if (leftInteractionHandler != null)
            leftInteractionHandler.RunInUpdates(0, () => { EndGrabMethod.Invoke(leftInteractionHandler, [false]); });

        var rightInteractionHandler = user.GetInteractionHandler(Chirality.Right);
        if (rightInteractionHandler != null)
            rightInteractionHandler.RunInUpdates(0, () => { EndGrabMethod.Invoke(rightInteractionHandler, [false]); });
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(InteractionHandler), "StartGrab")]
    private static bool PreventGrabbing_InteractionHandlerStartGrab_Prefix(InteractionHandler __instance)
    {
        return __instance.World == Userspace.UserspaceWorld ||
               !RestrainiteMod.IsRestricted(PreventionType.PreventGrabbing);
    }
}