using FrooxEngine;
using HarmonyLib;
using Restrainite.RestrictionTypes.Base;
using static FrooxEngine.VoiceMode;

namespace Restrainite.Patches;

[HarmonyPatch]
internal static class EnforceWhispering
{
    private static VoiceMode _originalVoiceMode = Whisper;

    internal static void Initialize()
    {
        Restrictions.EnforceWhispering.OnChanged += OnRestrictionChanged;
    }

    private static void OnRestrictionChanged(IRestriction restriction)
    {
        var user = Engine.Current.WorldManager.FocusedWorld.LocalUser;
        if (user == null) return;

        user.Root.Slot.RunInUpdates(0, () =>
        {
            var value = Restrictions.EnforceWhispering.IsRestricted;
            if (Restrictions.EnforceWhispering.IsRestricted)
            {
                if (user.VoiceMode is not (Normal or Shout or Broadcast)) return;
                _originalVoiceMode = user.VoiceMode;
                user.VoiceMode = Whisper;
            }
            else if (!value)
            {
                if (user.VoiceMode is not Whisper) return;
                user.VoiceMode = _originalVoiceMode;
            }
        });
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(User), nameof(User.VoiceMode), MethodType.Setter)]
    private static bool User_VoiceMode_Setter_Prefix(VoiceMode value, User __instance)
    {
        return !(__instance.IsLocalUser &&
                 Restrictions.EnforceWhispering.IsRestricted &&
                 value is Normal or Shout or Broadcast);
    }
}