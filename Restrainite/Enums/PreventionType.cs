using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Restrainite.Enums;

internal enum PreventionType
{
    PreventEquippingAvatar,
    PreventOpeningContextMenu,
    PreventUsingTools,
    PreventOpeningDash,
    PreventGrabbing,
    PreventHearing,
    EnforceSelectiveHearing,
    PreventLaserTouch,
    PreventPhysicalTouch,
    PreventSpeaking,
    EnforceWhispering,
    PreventRespawning,
    PreventEmergencyRespawning,
    PreventSwitchingWorld,
    ShowContextMenuItems,
    HideContextMenuItems,
    ShowDashScreens,
    HideDashScreens,
    PreventUserScaling,
    PreventCrouching,
    PreventJumping,
    PreventClimbing,
    PreventRunning,
    PreventChangeLocomotion,
    ResetUserScale,
    PreventLeavingAnchors,
    DisableNotifications,
    PreventSendingMessages,
    PreventInviteContact,
    PreventThirdPersonView,
    PreventSpawnObjects,
    PreventSaveItems,
    PreventMovement,
    PreventTurning,
    ShowUserAvatars,
    HideUserAvatars,
    AllowGrabbingBySlotTags,
    DenyGrabbingBySlotTags,
    AllowTouchingBySlotTags,
    DenyTouchingBySlotTags,
    PreventNonDashUserspaceInteraction,
    DisableNameplates,
    MovementSpeedMultiplier,
    MaximumLaserDistance
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
            PreventionType.MovementSpeedMultiplier or PreventionType.MaximumLaserDistance => true,
            _ => false
        };
    }

    internal static string GetDescription(this PreventionType type)
    {
        return type switch
        {
            PreventionType.PreventEquippingAvatar => "Prevents equipping in-world avatars or switching from inventory.",
            PreventionType.PreventOpeningContextMenu => "Prevents opening the context menu, and closes it if already opened.",
            PreventionType.PreventUsingTools => "Prevents equipping tools, and drops them if already equipped.",
            PreventionType.PreventOpeningDash => "Prevents opening the dashboard, and closes it if already opened.",
            PreventionType.PreventGrabbing => "Prevents grabbing objects physically/via laser, and drops any that are already grabbed.",
            PreventionType.PreventHearing => "Forces all other users voices to be muted.",
            PreventionType.EnforceSelectiveHearing => "When enabled, All users will be muted except those whose user IDs (not usernames) are in this list.",
            PreventionType.PreventLaserTouch => "Prevents any laser-based interaction.",
            PreventionType.PreventPhysicalTouch => "Prevents any physically-based interaction.",
            PreventionType.PreventSpeaking => "Forces the user to be muted.",
            PreventionType.EnforceWhispering => "Forces the user to only be able to talk in whisper mode (they can still mute themselves).",
            PreventionType.PreventRespawning => "Prevents respawning except for emergency respawning.",
            PreventionType.PreventEmergencyRespawning => "Prevents using the emergency respawn gesture (can still respawn via session users tab).",
            PreventionType.PreventSwitchingWorld => "Prevents starting a new world, joining another session, leaving the current world, or changing focus.",
            PreventionType.ShowContextMenuItems => "When enabled, any context menu items not in this list will be hidden.",
            PreventionType.HideContextMenuItems => "When enabled, any context menu items in this list will be hidden.",
            PreventionType.ShowDashScreens => "When enabled, any dashboard screens not in this list will be hidden.",
            PreventionType.HideDashScreens => "When enabled, any dashboard screens in this list will be hidden.",
            PreventionType.PreventUserScaling => "Prevents the user from rescaling themselves.",
            PreventionType.PreventCrouching => "Prevents crouching in desktop mode.",
            PreventionType.PreventJumping => "Prevents jumping, but does not prevent exiting anchors.",
            PreventionType.PreventClimbing => "Prevents climbing by grabbing the world or characters.",
            PreventionType.PreventRunning => "Prevents running, when using keyboard (double tap or shift) or gamepad controls (joystick press).",
            PreventionType.PreventChangeLocomotion => "Prevents the user from changing their locomotion mode.",
            PreventionType.ResetUserScale => "Utility variable that resets a user to their default scale.",
            PreventionType.PreventLeavingAnchors => "Prevents the user from leaving any anchor themselves.",
            PreventionType.DisableNotifications => "The user can’t see notifications anymore.",
            PreventionType.PreventSendingMessages => "The user can’t send messages to contacts.",
            PreventionType.PreventInviteContact => "The user can’t invite contacts to the current world.",
            PreventionType.PreventThirdPersonView => "Desktop users can’t switch to third person view anymore.",
            PreventionType.PreventSpawnObjects => "Prevents spawning objects into the current world.",
            PreventionType.PreventSaveItems => "Prevents saving items to the inventory.",
            PreventionType.PreventMovement => "Prevent the user being able to move around via VR controller or keyboard.",
            PreventionType.PreventTurning => "Prevent the user from turning his body via VR controller or look around via mouse or keyboard. The user is still able to look around in VR by turning his head. Turning can’t be restricted for Gamepad users.",
            PreventionType.ShowUserAvatars => "When enabled, only user avatars are shown, whose user ID (not usernames) is in the list.",
            PreventionType.HideUserAvatars => "When enabled, user avatars are hidden, whose user ID (not usernames) is in the list.",
            PreventionType.AllowGrabbingBySlotTags => "When enforced, only allows grabbing items with tags in this list.",
            PreventionType.DenyGrabbingBySlotTags => "When enforced, prevents grabbing any items with tags in this list. ",
            PreventionType.AllowTouchingBySlotTags => "When enabled, only allows interacting with items with tags in this list.",
            PreventionType.DenyTouchingBySlotTags => "When enabled, prevents interacting with any items with tags in this list. ",
            PreventionType.PreventNonDashUserspaceInteraction => "Prevents interacting with anything in userspace besides the dashboard (facet anchors, userspace inspectors, etc.).",
            PreventionType.DisableNameplates => "Hides all avatar nameplates.",
            PreventionType.MovementSpeedMultiplier => "When enabled, the movement speed is multiplied by a value set in a DynamicValueVariable<float> with the same name. The value is limited from 0.0 to 1.0. If the value is 1.0, the user moves with normal speed, if it’s lower, they are slowed down.",
            PreventionType.MaximumLaserDistance => "When enabled, the laser distance is limited by a value set in a DynamicValueVariable<float> with the same name. The value has a minimum of 0.0 and is the distance in global space.",
            _ => "(Invalid PreventionType)"
        };
    }
}