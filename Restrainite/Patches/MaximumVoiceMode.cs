using FrooxEngine;
using HarmonyLib;
using Restrainite.RestrictionTypes.Base;
using static FrooxEngine.VoiceMode;

namespace Restrainite.Patches;

[HarmonyPatch]
internal static class MaximumVoiceMode
{
    private static VoiceMode _originalVoiceMode = Whisper;
    private static VoiceMode _originalVoiceMode2 = Normal;
    private static VoiceMode _lastVoiceMode = Normal;

    internal static void Initialize()
    {
        Restrictions.EnforceWhispering.OnChanged += OnRestrictionChanged;
        Restrictions.MaximumVoiceMode.OnChanged += OnRestrictionChanged;
    }

    private static void OnRestrictionChanged(IRestriction restriction)
    {
        var user = Engine.Current.WorldManager.FocusedWorld.LocalUser;
        if (user == null) return;

        user.Root.Slot.RunSynchronously(() =>
        {
            ToggleMaximumVoiceMode(user);
            ToggleEnforceWhispering(user);
        });
    }

    private static void ToggleMaximumVoiceMode(User user)
    {
        if (Restrictions.MaximumVoiceMode.IsRestricted)
        {
            if (user.VoiceMode > Restrictions.MaximumVoiceMode.LowestVoiceMode.Value)
            {
                _originalVoiceMode2 = user.VoiceMode;
                user.VoiceMode = _lastVoiceMode = Restrictions.MaximumVoiceMode.LowestVoiceMode.Value;
            }
        }
        else
        {
            if (user.VoiceMode == _lastVoiceMode) user.VoiceMode = _originalVoiceMode2;
        }
    }
    
    private static void ToggleEnforceWhispering(User user)
    {
        if (Restrictions.EnforceWhispering.IsRestricted)
        {
            if (user.VoiceMode is not (Normal or Shout or Broadcast)) return;
            _originalVoiceMode = user.VoiceMode;
            user.VoiceMode = Whisper;
        }
        else
        {
            if (user.VoiceMode is not Whisper) return;
            user.VoiceMode = _originalVoiceMode;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(User), nameof(User.VoiceMode), MethodType.Setter)]
    private static bool User_VoiceMode_Setter_Prefix(VoiceMode value, User __instance)
    {
        return !(__instance.IsLocalUser &&
                 (
                     (Restrictions.EnforceWhispering.IsRestricted && value is Normal or Shout or Broadcast) ||
                     (Restrictions.MaximumVoiceMode.IsRestricted &&
                      value > Restrictions.MaximumVoiceMode.LowestVoiceMode.Value)
                 )
            );
    }
}