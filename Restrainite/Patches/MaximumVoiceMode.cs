using System.Runtime.CompilerServices;
using FrooxEngine;
using HarmonyLib;
using Restrainite.RestrictionTypes.Base;
using static FrooxEngine.VoiceMode;

namespace Restrainite.Patches;

[HarmonyPatch]
internal static class MaximumVoiceMode
{
    private static readonly ConditionalWeakTable<User, LimitedVoiceMode> LimitedVoiceModes = new();

    internal static void Initialize()
    {
        Restrictions.EnforceWhispering.OnChanged += OnRestrictionChanged;
        Restrictions.MaximumVoiceMode.OnChanged += OnRestrictionChanged;
    }

    private static void OnRestrictionChanged(IRestriction restriction)
    {
        var user = Engine.Current?.WorldManager?.FocusedWorld?.LocalUser;
        user?.World?.RunSynchronously(() => UpdateVoiceMode(user));
    }

    private static VoiceMode MaximumAllowedVoiceMode()
    {
        var maximum = Restrictions.MaximumVoiceMode.IsRestricted
            ? Restrictions.MaximumVoiceMode.LowestVoiceMode.Value
            : Broadcast;
        return Restrictions.EnforceWhispering.IsRestricted && maximum > Whisper ? Whisper : maximum;
    }

    private static void UpdateVoiceMode(User user)
    {
        // If the user selected another voice mode while being limited, then that is the one to keep.
        var current = user.VoiceMode;
        var original = LimitedVoiceModes.TryGetValue(user, out var limited) && limited.Applied == current
            ? limited.Original
            : current;

        var maximum = MaximumAllowedVoiceMode();
        if (original > maximum)
        {
            LimitedVoiceModes.AddOrUpdate(user, new LimitedVoiceMode(original, maximum));
            user.VoiceMode = maximum;
        }
        else
        {
            LimitedVoiceModes.Remove(user);
            user.VoiceMode = original;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(User), nameof(User.VoiceMode), MethodType.Setter)]
    private static bool User_VoiceMode_Setter_Prefix(VoiceMode value, User __instance)
    {
        return !__instance.IsLocalUser || value <= MaximumAllowedVoiceMode();
    }

    private sealed record LimitedVoiceMode(VoiceMode Original, VoiceMode Applied);
}