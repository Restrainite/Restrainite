using System.Collections.Generic;
using System.Reflection;
using FrooxEngine;
using HarmonyLib;

namespace Restrainite.Patches;

[HarmonyPatch]
internal static class PreventRespawning
{
    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(Slot), nameof(Slot.DestroyPreservingAssets),
            [typeof(Slot), typeof(bool)]);
        yield return AccessTools.Method(typeof(Slot), nameof(Slot.Destroy),
            [typeof(bool)]);
    }

    private static bool Prefix(Slot __instance)
    {
        if (PreventEmergencyRespawning.IsEmergencyRespawning(__instance.World)) return true;

        var userRootSlot = __instance.Engine?.WorldManager?.FocusedWorld?.LocalUser?.Root?.Slot;
        return __instance != userRootSlot || !Restrictions.PreventRespawning.IsRestricted;
    }
}