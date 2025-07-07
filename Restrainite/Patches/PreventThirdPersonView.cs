using FrooxEngine;
using HarmonyLib;

namespace Restrainite.Patches;

[HarmonyPatch]
internal static class PreventThirdPersonView
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(ScreenController), nameof(ScreenController.CanUseViewTargetting))]
    private static bool ScreenController_CanUseViewTargetting_Prefix(
        IViewTargettingController view, ref bool __result)
    {
        if (!Restrictions.PreventThirdPersonView.IsRestricted) return true;
        if (view is not (ThirdPersonTargettingController or FreeformTargettingController)) return true;

        __result = false;
        return false;
    }
}