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

        var user = Engine.Current?.WorldManager?.FocusedWorld?.LocalUser;
        user?.World?.RunSynchronously(() => CloseContextMenu(user));
    }

    private static void CloseContextMenu(User user)
    {
        var contextMenu = user.GetUserContextMenu();
        if (contextMenu?.CurrentSummoner is InteractionHandler interactionHandler && IsRestricted(interactionHandler))
            contextMenu.Close();
    }

    private static bool IsRestricted(InteractionHandler interactionHandler)
    {
        return Restrictions.PreventOpeningContextMenu.IsRestricted &&
               interactionHandler.World != Userspace.UserspaceWorld &&
               Restrictions.PreventOpeningContextMenu.Chirality.IsRestricted(interactionHandler.Side.Value);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(InteractionHandler), "TryOpenContextMenu")]
    private static bool InteractionHandler_TryOpenContextMenu_Prefix(
        InteractionHandler __instance)
    {
        return !IsRestricted(__instance);
    }
}
