using FrooxEngine;
using HarmonyLib;

namespace Restrainite.Patches;

[HarmonyPatch]
internal static class PreventSpawnObjects
{
    [ThreadStatic] private static bool _ignoreRestriction;

    internal static bool CanSpawnObjectsIgnoringRestriction(World world)
    {
        var previous = _ignoreRestriction;
        _ignoreRestriction = true;
        try
        {
            return world.CanSpawnObjects();
        }
        finally
        {
            _ignoreRestriction = previous;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(WorldPermissionsExtensoins), nameof(WorldPermissionsExtensoins.CanSpawnObjects))]
    private static bool WorldPermissionsExtensoins_CanSpawnObjects_Prefix(ref bool __result)
    {
        if (!Restrictions.PreventSpawnObjects.IsRestricted || _ignoreRestriction)
            return true;

        __result = false;
        return false;
    }
}