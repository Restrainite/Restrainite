using System.Collections.Generic;
using System.Reflection;
using FrooxEngine;
using HarmonyLib;

namespace Restrainite.Patches;

[HarmonyPatch]
internal static class DisableNotifications
{
    private static IEnumerable<MethodBase> TargetMethods()
    {
        return AccessTools.GetDeclaredMethods(typeof(NotificationPanel))
            .FindAll(info => "AddNotification".Equals(info.Name));
    }

    [HarmonyPrefix]
    private static bool NotificationPanel_AddNotification_Prefix()
    {
        return !Restrictions.DisableNotifications.IsRestricted;
    }
}