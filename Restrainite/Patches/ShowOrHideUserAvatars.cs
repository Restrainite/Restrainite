using System.Linq;
using System.Threading;
using FrooxEngine;
using HarmonyLib;
using Restrainite.Enums;
using Restrainite.States;

namespace Restrainite.Patches;

[HarmonyPatch]
internal static class ShowOrHideUserAvatars
{
    private static readonly ThreadLocal<bool> InUpdateBlocking = new();

    internal static void Initialize()
    {
        RestrainiteMod.BoolState.OnChanged += OnRestrictionChanged;
        RestrainiteMod.StringSetState.OnChanged += OnRestrictionChanged;
    }

    private static void OnRestrictionChanged(PreventionType preventionType, bool value)
    {
        if (preventionType is not (PreventionType.ShowUserAvatars or PreventionType.HideUserAvatars)) return;
        MarkAllUsersDirty();
    }

    private static void MarkAllUsersDirty()
    {
        var world = Engine.Current?.WorldManager?.FocusedWorld;
        world?.RunInUpdates(0, () =>
        {
            var userList = world.AllUsers;
            if (userList is null) return;
            foreach (var slot in userList.Select(user => user?.Root?.Slot))
                slot?.ForeachComponentInChildren<Component>(c => c?.MarkChangeDirty());
        });
    }

    private static void OnRestrictionChanged(PreventionType preventionType, ImmutableStringSet stringSet)
    {
        if (preventionType is not (PreventionType.ShowUserAvatars or PreventionType.HideUserAvatars) ||
            !RestrainiteMod.IsRestricted(preventionType)) return;

        MarkAllUsersDirty();
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
        if (RestrainiteMod.IsRestricted(PreventionType.ShowUserAvatars) &&
            !RestrainiteMod.GetStringSet(PreventionType.ShowUserAvatars).Contains(__instance.UserID))
        {
            __result = true;
            return;
        }

        if (!RestrainiteMod.IsRestricted(PreventionType.HideUserAvatars) ||
            !RestrainiteMod.GetStringSet(PreventionType.HideUserAvatars).Contains(__instance.UserID)) return;
        __result = true;
    }
}