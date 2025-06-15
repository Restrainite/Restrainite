using FrooxEngine.CommonAvatar;
using HarmonyLib;

namespace Restrainite.Patches;

[HarmonyPatch]
internal static class PreventLeavingAnchors
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(AvatarAnchor), nameof(AvatarAnchor.Release))]
    private static bool AvatarAnchor_Release_Prefix(AvatarAnchor __instance)
    {
        return __instance.Engine.WorldManager.FocusedWorld.LocalUser != __instance.AnchoredUser
               || !Restrictions.PreventLeavingAnchors.IsRestricted;
    }
}