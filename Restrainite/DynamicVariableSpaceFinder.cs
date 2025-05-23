using FrooxEngine;
using HarmonyLib;

namespace Restrainite;

internal static class DynamicVariableSpaceFinder
{
    [HarmonyPatch(typeof(DynamicVariableSpace), "UpdateName")]
    private static class DynamicVariableSpaceUpdateNamePatch
    {
        private static void Postfix(DynamicVariableSpace __instance)
        {
            DynamicVariableSpaceSync.UpdateList(__instance);
        }
    }

    [HarmonyPatch(typeof(DynamicVariableSpace), "OnDispose")]
    private static class DynamicVariableSpaceOnDisposePatch
    {
        private static void Postfix(DynamicVariableSpace __instance)
        {
            DynamicVariableSpaceSync.Remove(__instance);
        }
    }
}