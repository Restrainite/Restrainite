using FrooxEngine;
using HarmonyLib;
using Restrainite.RestrictionTypes.Base;

namespace Restrainite.Patches;

[HarmonyPatch]
internal static class PreventOpeningDash
{
    internal static void Initialize()
    {
        Restrictions.PreventOpeningDash.OnChanged += OnChanged;
    }

    private static void OnChanged(IRestriction restriction)
    {
        if (!Restrictions.PreventOpeningDash.IsRestricted)
            return;

        Userspace.Current.RunSynchronously(() =>
        {
            Userspace.UserspaceWorld.GetGloballyRegisteredComponent<UserspaceRadiantDash>().Open = false;
        });
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(UserspaceRadiantDash), nameof(UserspaceRadiantDash.Open), MethodType.Setter)]
    private static void UserspaceRadiantDash_Open_Setter_Prefix(ref bool value)
    {
        if (Restrictions.PreventOpeningDash.IsRestricted) value = false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(UserspaceRadiantDash), nameof(UserspaceRadiantDash.OpenContact))]
    private static bool UserspaceRadiantDash_OpenContact_Prefix()
    {
        return !Restrictions.PreventOpeningDash.IsRestricted;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(UserspaceRadiantDash), nameof(UserspaceRadiantDash.ToggleSessionControl))]
    private static bool UserspaceRadiantDash_ToggleSessionControl_Prefix()
    {
        return !Restrictions.PreventOpeningDash.IsRestricted;
    }
}