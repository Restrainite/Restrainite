using Elements.Core;
using FrooxEngine;
using HarmonyLib;

namespace Restrainite.Patches;

[HarmonyPatch]
internal static class PreventMovement
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(VR_LocomotionDirection), nameof(VR_LocomotionDirection.Evaluate))]
    private static bool VR_LocomotionDirection_Evaluate_Prefix(ref float3? __result)
    {
        if (!Restrictions.PreventMovement.IsRestricted) return true;

        __result = float3.Zero;
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ScreenLocomotionDirection), nameof(ScreenLocomotionDirection.Evaluate))]
    private static bool ScreenLocomotionDirection_Evaluate_Prefix(ref float3? __result)
    {
        if (!Restrictions.PreventMovement.IsRestricted) return true;

        __result = float3.Zero;
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(TeleportLocomotion), "ProcessInput")]
    private static bool TeleportLocomotion_ProcessInput_Prefix()
    {
        return !Restrictions.PreventMovement.IsRestricted;
    }
}