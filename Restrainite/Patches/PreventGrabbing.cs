using System.Reflection;
using FrooxEngine;
using HarmonyLib;
using ResoniteModLoader;
using Restrainite.RestrictionTypes.Base;

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

        Restrictions.PreventGrabbing.OnChanged += OnChanged;
    }

    private static void OnChanged(IRestriction restriction)
    {
        if (!Restrictions.PreventGrabbing.IsRestricted)
            return;

        if (EndGrabMethod == null) return;
        var user = Engine.Current?.WorldManager?.FocusedWorld?.LocalUser;
        if (user == null) return;
        var leftInteractionHandler = user.GetInteractionHandler(Chirality.Left);
        if (leftInteractionHandler != null && Restrictions.PreventGrabbing.Chirality.IsRestricted(Chirality.Left))
            leftInteractionHandler.RunInUpdates(0, () => { EndGrabMethod.Invoke(leftInteractionHandler, [false]); });

        var rightInteractionHandler = user.GetInteractionHandler(Chirality.Right);
        if (rightInteractionHandler != null && Restrictions.PreventGrabbing.Chirality.IsRestricted(Chirality.Right))
            rightInteractionHandler.RunInUpdates(0, () => { EndGrabMethod.Invoke(rightInteractionHandler, [false]); });
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(InteractionHandler), "StartGrab")]
    private static bool InteractionHandler_StartGrab_Prefix(InteractionHandler __instance)
    {
        return __instance.World == Userspace.UserspaceWorld ||
               !Restrictions.PreventGrabbing.IsRestricted ||
               !Restrictions.PreventGrabbing.Chirality.IsRestricted(__instance.Side.Value);
    }
}