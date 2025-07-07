using FrooxEngine;
using HarmonyLib;

namespace Restrainite.Patches;

[HarmonyPatch]
internal static class PreventPhysicalTouch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(RaycastTouchSource), "GetTouchable")]
    private static void RaycastTouchSource_GetTouchable_Postfix(ref ITouchable? __result)
    {
        if (__result?.World == Userspace.UserspaceWorld) return;
        if (Restrictions.PreventPhysicalTouch.IsRestricted) __result = null!;
    }
}