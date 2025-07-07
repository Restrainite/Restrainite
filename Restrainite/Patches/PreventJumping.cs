using FrooxEngine;
using HarmonyLib;

namespace Restrainite.Patches;

[HarmonyPatch]
internal static class PreventJumping
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(CharacterController), nameof(CharacterController.Jump), MethodType.Getter)]
    private static bool CharacterController_Jump_Getter_Prefix(ref bool __result)
    {
        if (!Restrictions.PreventJumping.IsRestricted) return true;
        __result = false;
        return false;
    }
}