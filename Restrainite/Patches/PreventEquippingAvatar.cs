using FrooxEngine;
using FrooxEngine.CommonAvatar;
using HarmonyLib;

namespace Restrainite.Patches;

[HarmonyPatch]
internal static class PreventEquippingAvatar
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(AvatarManager), nameof(AvatarManager.Equip))]
    private static bool AvatarManager_Equip_Prefix(ref bool __result)
    {
        if (!Restrictions.PreventEquippingAvatar.IsRestricted)
            return true;

        __result = false;
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(WorldPermissionsExtensoins), nameof(WorldPermissionsExtensoins.CanSwapAvatar))]
    private static bool WorldPermissionsExtensoins_CanSwapAvatar_Prefix(ref bool __result)
    {
        if (!Restrictions.PreventEquippingAvatar.IsRestricted)
            return true;

        __result = false;
        return false;
    }
}