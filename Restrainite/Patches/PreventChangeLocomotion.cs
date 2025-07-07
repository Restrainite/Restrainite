using FrooxEngine;
using HarmonyLib;

namespace Restrainite.Patches;

[HarmonyPatch]
internal static class PreventChangeLocomotion
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(LocomotionController), nameof(LocomotionController.CanUseAnyLocomotion))]
    private static bool LocomotionController_CanUseAnyLocomotion_Prefix(ref bool __result)
    {
        if (!Restrictions.PreventChangeLocomotion.IsRestricted) return true;
        __result = false;
        return false;
    }
}