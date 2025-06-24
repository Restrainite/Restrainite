using FrooxEngine;
using FrooxEngine.UIX;
using HarmonyLib;
using Restrainite.Enums;

namespace Restrainite.Patches;

[HarmonyPatch]
internal static class PreventEditMode
{
    internal static void Initialize()
    {
        RestrainiteMod.OnRestrictionChanged += OnRestrictionChanged;
    }

    private static void OnRestrictionChanged(PreventionType preventionType, bool value)
    {
        if (preventionType != PreventionType.PreventEditMode || !value) return;
        var world = Engine.Current?.WorldManager?.FocusedWorld;
        world?.RunSynchronously(() =>
        {
            var user = world.LocalUser;
            if (user == null) return;
            var mode = user.editMode.Value;
            if (mode != true) return;
            user.editMode.Value = false;
        });
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(User), nameof(User.EditMode), MethodType.Setter)]
    private static bool User_EditMode_Prefix()
    {
        return !RestrainiteMod.IsRestricted(PreventionType.PreventEditMode);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(SessionControlDialog), "OnCommonUpdate")]
    private static void SessionControlDialog_OnCommonUpdate_Postfix(SyncRef<Button> ____editMode)
    {
        if (RestrainiteMod.IsRestricted(PreventionType.PreventEditMode))
            ____editMode.Target.Enabled = false;
    }
}