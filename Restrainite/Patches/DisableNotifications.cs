using System.Reflection;
using FrooxEngine;
using HarmonyLib;

namespace Restrainite.Patches;

[HarmonyPatch]
internal static class DisableNotifications
{
    private static List<MethodInfo> TargetMethods()
    {
        return AccessTools.GetDeclaredMethods(typeof(NotificationPanel))
            .FindAll(info => "AddNotification".Equals(info.Name, StringComparison.Ordinal));
    }

    [HarmonyPrefix]
    private static bool NotificationPanel_AddNotification_Prefix()
    {
        return !Restrictions.DisableNotifications.IsRestricted;
    }
}