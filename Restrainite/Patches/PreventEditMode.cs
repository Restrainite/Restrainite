using FrooxEngine;
using FrooxEngine.UIX;
using HarmonyLib;
using Restrainite.RestrictionTypes.Base;

namespace Restrainite.Patches;

[HarmonyPatch]
internal static class PreventEditMode
{
    internal static void Initialize()
    {
        Restrictions.PreventEditMode.OnChanged += OnRestrictionChanged;
    }

    private static void OnRestrictionChanged(IRestriction restriction)
    {
        if (!Restrictions.PreventEditMode.IsRestricted) return;
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
    private static bool User_EditMode_Prefix()
    {
        return !Restrictions.PreventEditMode.IsRestricted;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(SessionControlDialog), "OnCommonUpdate")]
    private static void SessionControlDialog_OnCommonUpdate_Postfix(SyncRef<Button> ____editMode)
    {
        if (Restrictions.PreventEditMode.IsRestricted)
            ____editMode.Target.Enabled = false;
    }
}