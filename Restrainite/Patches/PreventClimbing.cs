using Elements.Core;
using FrooxEngine;
using HarmonyLib;

namespace Restrainite.Patches;

[HarmonyPatch]
internal static class PreventClimbing
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(GrabWorldLocomotion), "TryActivate")]
    private static bool GrabWorldLocomotion_TryActivate_Prefix(ref float3? ___currentAnchor)
    {
        if (!Restrictions.PreventClimbing.IsRestricted) return true;
        ___currentAnchor = null;
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(GrabWorldLocomotion), "CheckDeactivate")]
    private static bool GrabWorldLocomotion_CheckDeactivate_Prefix(ref float3? ___currentAnchor)
    {
        if (!Restrictions.PreventClimbing.IsRestricted) return true;
        ___currentAnchor = null;
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(PhysicalLocomotion), "CheckKeepGrip")]
    private static bool PhysicalLocomotion_CheckKeepGrip_Prefix(ref bool __result)
    {
        if (!Restrictions.PreventClimbing.IsRestricted) return true;
        __result = false;
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(PhysicalLocomotion), "CheckAquireGrip")]
    private static bool PhysicalLocomotion_CheckAquireGrip_Prefix()
    {
        return !Restrictions.PreventClimbing.IsRestricted;
    }
}