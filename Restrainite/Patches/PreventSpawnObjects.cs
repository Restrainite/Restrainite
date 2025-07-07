using FrooxEngine;
using HarmonyLib;

namespace Restrainite.Patches;

[HarmonyPatch]
internal static class PreventSpawnObjects
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(WorldPermissionsExtensoins), nameof(WorldPermissionsExtensoins.CanSpawnObjects))]
    private static bool WorldPermissionsExtensoins_CanSpawnObjects_Prefix(ref bool __result)
    {
        if (!Restrictions.PreventSpawnObjects.IsRestricted)
            return true;

        __result = false;
        return false;
    }
}