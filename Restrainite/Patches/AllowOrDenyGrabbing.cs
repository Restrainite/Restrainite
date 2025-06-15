using System.Collections.Generic;
using System.Reflection;
using FrooxEngine;
using HarmonyLib;

namespace Restrainite.Patches;

[HarmonyPatch]
internal static class AllowOrDenyGrabbing
{
    private static readonly SlotTagPermissionChecker SlotTagPermissionChecker = new(
        Restrictions.AllowGrabbingBySlotTags,
        Restrictions.DenyGrabbingBySlotTags);

    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(Grabbable), nameof(Grabbable.CanGrab));
        yield return AccessTools.Method(typeof(Draggable), nameof(Draggable.CanGrab));
        yield return AccessTools.Method(typeof(GrabInstancer), nameof(GrabInstancer.CanGrab));
    }

    private static void Postfix(IGrabbable __instance, ref bool __result)
    {
        if (__instance.World != Userspace.UserspaceWorld)
            __result &= SlotTagPermissionChecker.IsAllowed(__instance.Slot);
        else if (Restrictions.PreventNonDashUserspaceInteraction.IsRestricted)
            __result &= __instance.Slot.GetComponentInParents<UserspaceRadiantDash>() != null;
    }
}