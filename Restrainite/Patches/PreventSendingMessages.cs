using FrooxEngine;
using HarmonyLib;

namespace Restrainite.Patches;

[HarmonyPatch]
internal static class PreventSendingMessages
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(ContactsDialog), "TrySendMessage")]
    private static bool ContactsDialog_TrySendMessage_Prefix(ref bool __result)
    {
        if (!Restrictions.PreventSendingMessages.IsRestricted) return true;
        __result = false;
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ContactsDialog), "OnSendMessage")]
    private static bool ContactsDialog_OnSendMessage_Prefix()
    {
        return !Restrictions.PreventSendingMessages.IsRestricted;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ContactsDialog), "OnStartRecordingVoiceMessage")]
    private static bool ContactsDialog_OnStartRecordingVoiceMessage_Prefix()
    {
        return !Restrictions.PreventSendingMessages.IsRestricted;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ContactsDialog), "OnStopRecordingVoiceMessage")]
    private static bool ContactsDialog_OnStopRecordingVoiceMessage_Prefix()
    {
        return !Restrictions.PreventSendingMessages.IsRestricted;
    }
}