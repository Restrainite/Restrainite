using System.Threading;
using FrooxEngine.CommonAvatar;
using HarmonyLib;

namespace Restrainite.Patches;

[HarmonyPatch]
internal static class PreventLeavingAnchors
{
    private static readonly ThreadLocal<bool> IsAnchoring = new();

    [HarmonyPrefix]
    [HarmonyPatch(typeof(AvatarAnchor), nameof(AvatarAnchor.Release))]
    private static bool AvatarAnchor_Release_Prefix(AvatarAnchor __instance)
    {
        return __instance.Engine.WorldManager.FocusedWorld.LocalUser != __instance.AnchoredUser
               || IsAnchoring.Value || !Restrictions.PreventLeavingAnchors.IsRestricted;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(AvatarAnchor), nameof(AvatarAnchor.Anchor), typeof(AvatarManager))]
    private static void AvatarAnchor_Anchor_Prefix()
    {
        IsAnchoring.Value = true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(AvatarAnchor), nameof(AvatarAnchor.Anchor), typeof(AvatarManager))]
    private static void AvatarAnchor_Anchor_Postfix()
    {
        IsAnchoring.Value = false;
    }
}