using FrooxEngine;
using HarmonyLib;

namespace Restrainite.Patches;

[HarmonyPatch]
internal static class PreventUsingTools
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(InteractionHandler), nameof(InteractionHandler.CanEquip))]
    private static bool InteractionHandler_CanEquip_Postfix(bool result, InteractionHandler __instance)
    {
        if (__instance.World == Userspace.UserspaceWorld) return result;

        return !Restrictions.PreventUsingTools.IsRestricted && result;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(InteractionHandler), nameof(InteractionHandler.CanKeepEquipped))]
    private static bool InteractionHandler_CanKeepEquipped_Postfix(bool result,
        InteractionHandler __instance)
    {
        if (__instance.World == Userspace.UserspaceWorld) return result;

        return !Restrictions.PreventUsingTools.IsRestricted && result;
    }
}