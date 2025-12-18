using FrooxEngine;
using HarmonyLib;

namespace Restrainite.Patches;

[HarmonyPatch]
internal static class MaximumLaserDistance
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(InteractionHandler), nameof(InteractionHandler.MaxLaserDistance), MethodType.Getter)]
    private static void InteractionHandler_MaxLaserDistance_Postfix(ref float __result, InteractionHandler __instance)
    {
        if (!Restrictions.MaximumLaserDistance.IsRestricted ||
            !Restrictions.MaximumLaserDistance.Chirality.IsRestricted(__instance.Side.Value)
            || __result < float.MaxValue)
            return;

        var distance = Restrictions.MaximumLaserDistance.LowestFloat.Value;
        if (float.IsNaN(distance)) return;
        if (distance > __result) return;
        __result = distance;
    }
}