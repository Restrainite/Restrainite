using System;
using System.Collections.Concurrent;
using System.Linq;
using Elements.Core;
using FrooxEngine;
using FrooxEngine.CommonAvatar;
using HarmonyLib;

namespace Restrainite.Patches;

[HarmonyPatch]
internal static class PreventPhysicalTouch
{
    private static readonly ConcurrentDictionary<RefID, WeakReference<AvatarHandDataAssigner>> HandDataAssigners = [];

    [HarmonyPostfix]
    [HarmonyPatch(typeof(RaycastTouchSource), "GetTouchable")]
    private static void RaycastTouchSource_GetTouchable_Postfix(ref ITouchable? __result, RaycastTouchSource __instance)
    {
        if (__result?.World == Userspace.UserspaceWorld) return;
        if (!Restrictions.PreventPhysicalTouch.IsRestricted) return;
        if (Restrictions.PreventPhysicalTouch.Chirality.Value == null ||
            HandDataAssigners
                .Any(entry =>
                    entry.Value.TryGetTarget(out var avatarHandDataAssigner) &&
                    avatarHandDataAssigner != null &&
                    avatarHandDataAssigner.TouchSource.Target == __instance &&
                    avatarHandDataAssigner.Chirality.Value == Restrictions.PreventPhysicalTouch.Chirality.Value))
            __result = null!;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(AvatarHandDataAssigner), nameof(AvatarHandDataAssigner.OnEquip))]
    private static void AvatarHandDataAssigner_OnEquip_Postfix(AvatarHandDataAssigner __instance)
    {
        if (!HandDataAssigners.ContainsKey(__instance.ReferenceID))
            HandDataAssigners[__instance.ReferenceID] = new WeakReference<AvatarHandDataAssigner>(__instance);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(AvatarHandDataAssigner), nameof(AvatarHandDataAssigner.OnDequip))]
    private static void AvatarHandDataAssigner_OnDequip_Postfix(AvatarHandDataAssigner __instance)
    {
        HandDataAssigners.TryRemove(__instance.ReferenceID, out _);
        HandDataAssigners.Where(entry =>
                !entry.Value.TryGetTarget(out var avatarHandDataAssigner) || avatarHandDataAssigner == null)
            .Select(entry => entry.Key)
            .ToList()
            .ForEach(entry => HandDataAssigners.TryRemove(entry, out _));
    }
}