using System.Collections.Generic;
using System.Reflection;
using FrooxEngine;
using HarmonyLib;
using Restrainite.Enums;

namespace Restrainite.Patches;

[HarmonyPatch]
internal static class AllowOrDenyGrabbing
{
    private static readonly SlotTagPermissionChecker SlotTagPermissionChecker = new(
        PreventionType.AllowGrabbingBySlotTags, PreventionType.DenyGrabbingBySlotTags);

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
        else if (RestrainiteMod.IsRestricted(PreventionType.PreventNonDashUserspaceInteraction))
            __result &= __instance.Slot.GetComponentInParents<UserspaceRadiantDash>() != null;
    }
}