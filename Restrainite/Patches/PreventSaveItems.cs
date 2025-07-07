using FrooxEngine;
using HarmonyLib;

namespace Restrainite.Patches;

[HarmonyPatch]
internal static class PreventSaveItems
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(WorldPermissionsExtensoins), nameof(WorldPermissionsExtensoins.CanSaveItems))]
    private static bool WorldPermissionsExtensoins_CanSaveItems_Prefix(ref bool __result)
    {
        if (!Restrictions.PreventSaveItems.IsRestricted)
            return true;

        __result = false;
        return false;
    }
}