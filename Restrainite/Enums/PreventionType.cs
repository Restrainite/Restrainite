using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Restrainite.Enums;

internal enum PreventionType
{
    AllowGrabbingBySlotTags,
    AllowHearingBySlotTags,
    AllowTouchingBySlotTags,
    AlwaysHearSelectedUsers,
    DenyGrabbingBySlotTags,
    DenyHearingBySlotTags,
    DenyTouchingBySlotTags,
    DisableNameplates,
    DisableNotifications,
    DisableVrTrackers,
    EnforceSelectiveHearing,
    EnforceWhispering,
    HearingVolume,
    HideContextMenuItems,
    HideDashScreens,
    HideOthersContextMenus,
    HideUserAvatars,
    MaximumHearingDistance,
    MaximumLaserDistance,
    MovementSpeedMultiplier,
    PreventChangeLocomotion,
    PreventClimbing,
    PreventCrouching,
    PreventEditMode,
    PreventEmergencyRespawning,
    PreventEquippingAvatar,
    PreventGrabbing,
    PreventHearing,
    PreventHearingOfSounds,
    PreventHearingOfUsers,
    PreventInviteContact,
    PreventJumping,
    PreventLaserTouch,
    PreventLeavingAnchors,
    PreventMovement,
    PreventNonDashUserspaceInteraction,
    PreventOpeningContextMenu,
    PreventOpeningDash,
    PreventPhysicalTouch,
    PreventReading,
    PreventRespawning,
    PreventRunning,
    PreventSaveItems,
    PreventSendingMessages,
    PreventSpawnObjects,
    PreventSpeaking,
    PreventSwitchingWorld,
    PreventThirdPersonView,
    PreventTurning,
    PreventUserScaling,
    PreventUsingTools,
    ResetUserScale,
    ShowContextMenuItems,
    ShowDashScreens,
    ShowUserAvatars,
    SpeakingVolume,
    TrackerMovementSpeed
}

internal static class PreventionTypes
{
    internal static readonly IEnumerable<PreventionType> List =
        Enum.GetValues(typeof(PreventionType)).Cast<PreventionType>();

    internal static readonly int Max = (int)List.Max() + 1;

    private static readonly Dictionary<PreventionType, string> Dictionary =
        List.ToDictionary(l => l,
            l => Regex.Replace(l.ToString(), "([a-z])([A-Z])", "$1 $2"));

    private static readonly Dictionary<string, PreventionType> NameToPreventionType =
        List.ToDictionary(l => Dictionary[l], l => l);

    internal static string ToExpandedString(this PreventionType type)
    {
        return Dictionary[type];
    }

    internal static bool TryParsePreventionType(this string preventionTypeString, out PreventionType preventionType)
    {
        return NameToPreventionType.TryGetValue(preventionTypeString, out preventionType);
    }

    internal static bool IsFloatType(this PreventionType type)
    {
        return type switch
        {
            PreventionType.MovementSpeedMultiplier or
                PreventionType.MaximumLaserDistance or
                PreventionType.MaximumHearingDistance or
                PreventionType.HearingVolume or
                PreventionType.SpeakingVolume or
                PreventionType.TrackerMovementSpeed => true,
            _ => false
        };
    }

    internal static bool IsStringSetType(this PreventionType type)
    {
        return type switch
        {
            PreventionType.EnforceSelectiveHearing or
                PreventionType.ShowContextMenuItems or
                PreventionType.HideContextMenuItems or
                PreventionType.ShowDashScreens or
                PreventionType.HideDashScreens or
                PreventionType.ShowUserAvatars or
                PreventionType.HideUserAvatars or
                PreventionType.AllowGrabbingBySlotTags or
                PreventionType.DenyGrabbingBySlotTags or
                PreventionType.AllowTouchingBySlotTags or
                PreventionType.DenyTouchingBySlotTags or
                PreventionType.AllowHearingBySlotTags or
                PreventionType.DenyHearingBySlotTags => true,
            _ => false
        };
    }

    internal static string GetDescription(this PreventionType type)
    {
        return type switch
        {
            // Should others be able to control the restriction ...
            PreventionType.PreventEquippingAvatar => "Should others be able to prevent you from equipping avatars?",
            PreventionType.PreventOpeningContextMenu =>
                "Should others be able to prevent you from opening your context menu?",
            PreventionType.PreventUsingTools => "Should others be able to prevent you from equipping tools?",
            PreventionType.PreventOpeningDash => "Should others be able to prevent you from opening the dashboard?",
            PreventionType.PreventGrabbing => "Should others be able to prevent you from grabbing objects?",
            PreventionType.PreventHearing => "Should others be able to mute all audio sources including voices?",
            PreventionType.EnforceSelectiveHearing =>
                "Should others be able to limit the voices you can hear to specific players?",
            PreventionType.PreventLaserTouch =>
                "Should others be able to prevent you from any laser-based interaction?",
            PreventionType.PreventPhysicalTouch =>
                "Should others be able to prevent you from any physically-based interaction?",
            PreventionType.PreventSpeaking => "Should others be able to forcefully mute you?",
            PreventionType.EnforceWhispering =>
                "Should others be able to forcefully make you whisper? (You can still mute yourself.)",
            PreventionType.PreventRespawning =>
                "Should others be able to prevent you from respawning? (Except for emergency respawning.)",
            PreventionType.PreventEmergencyRespawning =>
                "Should others be able to prevent you from using the emergency respawn gesture?",
            PreventionType.PreventSwitchingWorld =>
                "Should others be able to prevent you from starting a new world, joining another session, leaving the current world, or changing focus?",
            PreventionType.ShowContextMenuItems =>
                "Should others be able to hide all context menu items except for specific ones? (Inverse of Hide Context Menu Items.)",
            PreventionType.HideContextMenuItems => "Should others be able to hide specific context menu items?",
            PreventionType.ShowDashScreens =>
                "Should others be able to hide all dashboard screens except for specific ones? (Inverse of Hide Dash Screens, excludes Exit screen.)",
            PreventionType.HideDashScreens =>
                "Should others be able to hide specific dashboard screens? (Excluding the Exit screen.)",
            PreventionType.PreventUserScaling => "Should others be able to prevent you from rescaling yourself?",
            PreventionType.PreventCrouching => "Should others be able to prevent you from crouching in desktop mode?",
            PreventionType.PreventJumping =>
                "Should others be able to prevent you from jumping? (Does not prevent exiting anchors.)",
            PreventionType.PreventClimbing =>
                "Should others be able to prevent you from climbing by grabbing the world or characters?",
            PreventionType.PreventRunning =>
                "Should others be able to prevent you from sprinting/running? (On desktop, this disables the sprint input, and in VR, you can't use both joysticks to move faster.)",
            PreventionType.PreventChangeLocomotion =>
                "Should others be able to prevent you from changing your locomotion mode?",
            PreventionType.ResetUserScale => "Should others be able to force you back to your default avatar scale?",
            PreventionType.PreventLeavingAnchors =>
                "Should others be able to prevent you from leaving seats/anchors yourself?",
            PreventionType.DisableNotifications => "Should others be able to disable your notifications?",
            PreventionType.PreventSendingMessages =>
                "Should others be able to prevent you from sending messages to contacts?",
            PreventionType.PreventInviteContact =>
                "Should others be able to prevent you from inviting contacts to your current world?",
            PreventionType.PreventThirdPersonView =>
                "Should others be able to prevent you from switching to third-person in desktop mode?",
            PreventionType.PreventSpawnObjects => "Should others be able to prevent you from spawning objects?",
            PreventionType.PreventSaveItems =>
                "Should others be able to prevent you from saving items to your inventory?",
            PreventionType.PreventMovement => "Should others be able to prevent you from walking/moving by yourself?",
            PreventionType.PreventTurning =>
                "Should others be able to prevent you from turning your body? (You can still look around in VR.)",
            PreventionType.ShowUserAvatars =>
                "Should others be able to hide all players except for specific ones? (Inverse of Hide User Avatars.)",
            PreventionType.HideUserAvatars => "Should others be able to hide specific players?",
            PreventionType.AllowGrabbingBySlotTags =>
                "Should others be able to prevent you from grabbing all objects except for those with a specific tag? (Inverse of Prevent Grabbing By Slot Tags.)",
            PreventionType.DenyGrabbingBySlotTags =>
                "Should others be able to prevent you from grabbing objects with a specific tag?",
            PreventionType.AllowTouchingBySlotTags =>
                "Should others be able to prevent you from touching all objects except for those with a specific tag? (Inverse of Prevent Touching By Slot Tags.)",
            PreventionType.DenyTouchingBySlotTags =>
                "Should others be able to prevent you from touching objects with a specific tag?",
            PreventionType.PreventNonDashUserspaceInteraction =>
                "Should others be able to prevent you from interacting with anything in userspace besides the dashboard and notice popups? (Facet anchors, userspace inspectors, etc. can no longer be interacted with.)",
            PreventionType.DisableNameplates => "Should others be able to disable avatar nameplates for you?",
            PreventionType.MovementSpeedMultiplier => "Should others be able to limit your movement speed?",
            PreventionType.MaximumLaserDistance =>
                "Should others be able to limit the distance on objects you can interact with? (Limiting how far your interaction laser reaches.)",
            PreventionType.PreventHearingOfSounds =>
                "Should others be able to mute all sounds except other users voices?",
            PreventionType.PreventHearingOfUsers => "Should others be able to mute all user voices for you?",
            PreventionType.MaximumHearingDistance =>
                "Should others be able to limit how far away you can hear sounds/audio from?",
            PreventionType.AllowHearingBySlotTags =>
                "Should others be able to prevent you from hearing all audio sources except for those with a specific tag? (Inverse of Deny Hearing By Slot Tags.)",
            PreventionType.DenyHearingBySlotTags =>
                "Should others be able to prevent you from hearing audio sources with a specific tag?",
            PreventionType.HearingVolume => "Should others be able to make all sounds/audio quieter for you?",
            PreventionType.PreventReading =>
                "Should others be able to scramble all text on screen, making you unable to read?",
            PreventionType.SpeakingVolume => "Should others be able to make your voice quieter to everyone else?",
            PreventionType.TrackerMovementSpeed =>
                "Should others be able to slow down or freeze your full-body trackers? (Including VR controllers.)",
            PreventionType.DisableVrTrackers =>
                "Should others be able to disable your VR controllers and full-body trackers?",
            PreventionType.PreventEditMode =>
                "Should others be able to disable your ability to enable Edit Mode (F2)?",
            PreventionType.HideOthersContextMenus =>
                "Should others be able to prevent you from seeing other users context menus.",
            PreventionType.AlwaysHearSelectedUsers =>
                "Should others be able to control, that you can hear selected users, even if other restrictions forbid it.",
            _ => "(Invalid PreventionType)"
        };
    }

    internal static T[] CreateDefaultArray<T>(T defaultValue)
    {
        var array = new T[Max];
        for (var i = 0; i < Max; i++) array[i] = defaultValue;
        return array;
    }
}