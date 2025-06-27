using System;
using System.Collections.Generic;
using System.Reflection;
using FrooxEngine;
using HarmonyLib;
using Restrainite.RestrictionTypes.Base;
using ResoniteModLoader;

namespace Restrainite.Patches;

[HarmonyPatch]
internal static class DisableNameplates
{
    private static readonly List<WeakReference<AvatarNameplateVisibilityDriver>> AvatarNameplateVisibilityDriverList =
        [];

    private static readonly MethodInfo? UpdateVisibility =
        AccessTools.Method(typeof(AvatarNameplateVisibilityDriver), "UpdateVisibility");

    internal static void Initialize()
    {
        if (UpdateVisibility == null)
        {
            ResoniteMod.Error(RestrainiteMod.LogReportUrl +
                              " Failed to find method AvatarNameplateVisibilityDriver.UpdateVisibility");
            RestrainiteMod.SuccessfullyPatched = false;
        }

        Restrictions.DisableNameplates.OnChanged += OnRestrictionChanged;
    }

    private static void OnRestrictionChanged(IRestriction restriction)
    {
        foreach (var avatarNameplateVisibilityDriver in AvatarNameplateVisibilityDriverList)
            if (avatarNameplateVisibilityDriver.TryGetTarget(out var avatarNameplateVisibilityDriverInstance) &&
                avatarNameplateVisibilityDriverInstance != null)
                UpdateVisibility?.Invoke(avatarNameplateVisibilityDriverInstance, []);

        AvatarNameplateVisibilityDriverList.RemoveAll(reference =>
            !reference.TryGetTarget(out var target) || target == null
        );
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(AvatarNameplateVisibilityDriver), nameof(AvatarNameplateVisibilityDriver.ShouldBeVisible),
        MethodType.Getter)]
    private static bool AvatarNameplateVisibilityDriver_ShouldBeVisiblePrefix(ref bool __result)
    {
        if (!Restrictions.DisableNameplates.IsRestricted) return true;
        __result = false;
        return false;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(AvatarNameplateVisibilityDriver), "OnAwake")]
    private static void AvatarNameplateVisibilityDriver_OnAwakePostfix(AvatarNameplateVisibilityDriver __instance)
    {
        AvatarNameplateVisibilityDriverList.Add(new WeakReference<AvatarNameplateVisibilityDriver>(__instance));
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(AvatarNameplateVisibilityDriver), "OnDispose")]
    private static void AvatarNameplateVisibilityDriver_OnDisposePostfix(AvatarNameplateVisibilityDriver __instance)
    {
        AvatarNameplateVisibilityDriverList.RemoveAll(reference =>
            !reference.TryGetTarget(out var target) || target == null || target == __instance
        );
    }
}