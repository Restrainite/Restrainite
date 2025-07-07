using FrooxEngine;
using HarmonyLib;

namespace Restrainite.Patches;

[HarmonyPatch]
internal static class AllowOrDenyTouching
{
    private static readonly SlotTagPermissionChecker SlotTagPermissionChecker = new(
        Restrictions.AllowTouchingBySlotTags,
        Restrictions.DenyTouchingBySlotTags);

    [HarmonyPostfix]
    [HarmonyPatch(typeof(TouchablePermissionsExtensions), nameof(TouchablePermissionsExtensions.CanTouch))]
    private static void TouchablePermissionsExtensions_CanTouch_Postfix(ITouchable touchable, ref bool __result)
    {
        if (touchable.World != Userspace.UserspaceWorld)
            __result &= SlotTagPermissionChecker.IsAllowed(touchable.Slot);
        else if (Restrictions.PreventNonDashUserspaceInteraction.IsRestricted)
            __result &= touchable.Slot.GetComponentInParents<UserspaceRadiantDash>() != null ||
                        touchable.Slot.GetComponentInParents<ContextMenu>() != null ||
                        touchable.Slot.GetComponentInParents<NoticeDisplayInterface>() != null;
        // ^ Needed so users can dismiss the 'All preset warning' popup.
    }
}