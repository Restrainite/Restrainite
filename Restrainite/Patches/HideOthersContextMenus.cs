using FrooxEngine;
using HarmonyLib;

namespace Restrainite.Patches;

[HarmonyPatch]
internal static class HideOthersContextMenus
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(ContextMenu), nameof(ContextMenu.Lerp), MethodType.Getter)]
    private static bool ContextMenu_Lerp_Prefix(ContextMenu __instance, ref float __result)
    {
        if (!Restrictions.HideOthersContextMenus.IsRestricted ||
            __instance.IsUnderLocalUser) return true;
        __result = 0.0f;
        return false;
    }
}