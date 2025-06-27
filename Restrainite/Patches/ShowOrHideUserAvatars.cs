using System.Linq;
using System.Threading;
using FrooxEngine;
using HarmonyLib;
using Restrainite.RestrictionTypes.Base;

namespace Restrainite.Patches;

[HarmonyPatch]
internal static class ShowOrHideUserAvatars
{
    private static readonly ThreadLocal<bool> InUpdateBlocking = new();

    internal static void Initialize()
    {
        Restrictions.ShowUserAvatars.OnChanged += OnRestrictionChanged;
        Restrictions.HideUserAvatars.OnChanged += OnRestrictionChanged;
    }

    private static void OnRestrictionChanged(IRestriction restriction)
    {
        MarkAllUsersDirty();
    }

    private static void MarkAllUsersDirty()
    {
        var world = Engine.Current?.WorldManager?.FocusedWorld;
        world?.RunSynchronously(() =>
        {
            var userList = world.AllUsers;
            if (userList is null) return;
            foreach (var slot in userList.Select(user => user?.Root?.Slot))
                slot?.ForeachComponentInChildren<Component>(c => c?.MarkChangeDirty());
        });
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(User), nameof(User.UpdateBlocking))]
    private static void User_UpdateBlocking_Prefix()
    {
        InUpdateBlocking.Value = true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(User), nameof(User.UpdateBlocking))]
    private static void User_UpdateBlocking_Postfix()
    {
        InUpdateBlocking.Value = false;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(User), nameof(User.IsRenderingLocallyBlocked), MethodType.Getter)]
    private static void User_IsRenderingLocallyBlocked_Postfix(ref bool __result, User __instance)
    {
        if (InUpdateBlocking.Value || __result || __instance == __instance.LocalUser) return;
        if (Restrictions.ShowUserAvatars.IsRestricted &&
            !Restrictions.ShowUserAvatars.SetContains(__instance.UserID))
        {
            __result = true;
            return;
        }

        if (!Restrictions.HideUserAvatars.IsRestricted ||
            !Restrictions.HideUserAvatars.SetContains(__instance.UserID)) return;
        __result = true;
    }
}