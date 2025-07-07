using FrooxEngine;
using HarmonyLib;

namespace Restrainite.Patches;

[HarmonyPatch]
internal static class PreventInviteContact
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(ContactsDialog), "OnInviteContact")]
    private static bool ContactsDialog_OnInviteContact_Prefix()
    {
        return !Restrictions.PreventInviteContact.IsRestricted;
    }
}