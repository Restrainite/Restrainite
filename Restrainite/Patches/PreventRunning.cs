using Elements.Core;
using FrooxEngine;
using HarmonyLib;
using Restrainite.Enums;

namespace Restrainite.Patches;

[HarmonyPatch]
internal static class PreventRunning
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(ScreenLocomotionDirection), nameof(ScreenLocomotionDirection.Evaluate))]
    private static void ScreenLocomotionDirection_Evaluate_Prefix(ScreenLocomotionDirection __instance,
        out float __state)
    {
        __state = __instance.FastMultiplier;
        if (RestrainiteMod.IsRestricted(PreventionType.PreventRunning)) __instance.FastMultiplier = 1.0f;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ScreenLocomotionDirection), nameof(ScreenLocomotionDirection.Evaluate))]
    private static void ScreenLocomotionDirection_Evaluate_Postfix(ScreenLocomotionDirection __instance, float __state)
    {
        __instance.FastMultiplier = __state;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(VR_LocomotionDirection), nameof(VR_LocomotionDirection.Evaluate))]
    private static void VR_LocomotionDirection_Evaluate_Postfix(ref float3? __result)
    {
        if (!RestrainiteMod.IsRestricted(PreventionType.PreventRunning) || __result == null) return;

        var normalized = __result.Value.GetNormalized(out var magnitude);
        if (magnitude > 1.0f) __result = normalized;
    }
}