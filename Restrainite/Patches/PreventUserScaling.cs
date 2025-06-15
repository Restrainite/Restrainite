using FrooxEngine;
using FrooxEngine.CommonAvatar;
using HarmonyLib;
using Restrainite.RestrictionTypes.Base;

namespace Restrainite.Patches;

[HarmonyPatch]
internal static class PreventUserScaling
{
    internal static void Initialize()
    {
        Restrictions.ResetUserScale.OnChanged += OnRestrictionChanged;
    }

    private static void OnRestrictionChanged(IRestriction restriction)
    {
        if (!Restrictions.ResetUserScale.IsRestricted) return;
        var user = Engine.Current.WorldManager.FocusedWorld.LocalUser;
        if (user == null) return;
        var activeUserRoot = user.Root.Slot.ActiveUserRoot;
        activeUserRoot.RunInUpdates(0, () => activeUserRoot.SetUserScale(activeUserRoot.GetDefaultScale(), 0.25f));
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(LocomotionController), nameof(LocomotionController.CanScale), MethodType.Getter)]
    private static bool LocomotionController_CanScale_Getter_Prefix(ref bool __result)
    {
        if (!Restrictions.PreventUserScaling.IsRestricted && !Restrictions.ResetUserScale.IsRestricted) return true;
        __result = false;
        return false;
    }
}