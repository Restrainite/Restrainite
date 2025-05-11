using FrooxEngine;
using HarmonyLib;
using Restrainite.Enums;

namespace Restrainite.Patches;

[HarmonyPatch]
internal static class AllowOrDenyTouching
{
    private static readonly SlotTagPermissionChecker SlotTagPermissionChecker = new(
        PreventionType.AllowTouchingBySlotTags, PreventionType.DenyTouchingBySlotTags);

    [HarmonyPostfix]
    [HarmonyPatch(typeof(TouchablePermissionsExtensions), nameof(TouchablePermissionsExtensions.CanTouch))]
    private static void TouchablePermissionsExtensions_CanTouch_Postfix(ITouchable touchable, ref bool __result)
    {
        if (touchable.World != Userspace.UserspaceWorld)
            __result &= SlotTagPermissionChecker.IsAllowed(touchable.Slot);
        else if (RestrainiteMod.IsRestricted(PreventionType.PreventNonDashUserspaceInteraction))
            __result &= touchable.Slot.GetComponentInParents<UserspaceRadiantDash>() != null ||
                        touchable.Slot.GetComponentInParents<ContextMenu>() != null;
    }
}