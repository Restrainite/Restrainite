using System.Reflection;
using FrooxEngine;
using HarmonyLib;

namespace Restrainite.Patches;

[HarmonyPatch]
internal static class PreventInviteContact
{
    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(ContactsDialog), "OnInviteContact");
        yield return AccessTools.Method(typeof(ContactsDialog), "InviteContact");
        yield return AccessTools.Method(typeof(InviteRequestItem), "SendDirectInvite");
        yield return AccessTools.Method(typeof(InviteRequestItem), "ForwardInviteRequestToHost");
        yield return AccessTools.Method(typeof(InviteRequestItem), "GrantInvite");
    }

    private static bool Prefix()
    {
        return !Restrictions.PreventInviteContact.IsRestricted;
    }
}