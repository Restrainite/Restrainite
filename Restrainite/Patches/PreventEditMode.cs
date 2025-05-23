using FrooxEngine;
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
        world?.RunInUpdates(0, () =>
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
    private static bool User_CanEnableEditMode_Prefix()
    {
        return !RestrainiteMod.IsRestricted(PreventionType.PreventEditMode);
    }
}