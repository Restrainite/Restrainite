using FrooxEngine;
using HarmonyLib;
using Restrainite.RestrictionTypes.Base;

namespace Restrainite.Patches;

[HarmonyPatch]
internal static class PreventOpeningContextMenu
{
    internal static void Initialize()
    {
        Restrictions.PreventOpeningContextMenu.OnChanged += OnChanged;
    }

    private static void OnChanged(IRestriction restriction)
    {
        if (!Restrictions.PreventOpeningContextMenu.IsRestricted) return;

        var user = Engine.Current.WorldManager.FocusedWorld.LocalUser;
        user.Root.RunSynchronously(() => user.CloseContextMenu(null!));
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(InteractionHandler), "TryOpenContextMenu")]
    private static bool InteractionHandler_TryOpenContextMenu_Prefix(
        InteractionHandler __instance)
    {
        return __instance.World == Userspace.UserspaceWorld ||
               !Restrictions.PreventOpeningContextMenu.IsRestricted;
    }
}