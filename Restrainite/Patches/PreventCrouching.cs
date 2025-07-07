using FrooxEngine;
using HarmonyLib;

namespace Restrainite.Patches;

[HarmonyPatch]
internal static class PreventCrouching
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(LocomotionController), nameof(LocomotionController.CanCrouch), MethodType.Getter)]
    private static bool LocomotionController_CanCrouch_Getter_Prefix(ref bool __result)
    {
        if (!Restrictions.PreventCrouching.IsRestricted) return true;
        __result = false;
        return false;
    }
}