using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FrooxEngine;
using ResoniteModLoader;
using Restrainite.RestrictionTypes;
using Restrainite.RestrictionTypes.Base;

namespace Restrainite;

internal static class Restrictions
{
    private static readonly Dictionary<string, IRestriction> NameToRestriction = new();

    internal static readonly IRestriction[] All;
    internal static readonly AllowGrabbingBySlotTags AllowGrabbingBySlotTags = new();
    internal static readonly AllowHearingBySlotTags AllowHearingBySlotTags = new();
    internal static readonly AllowTouchingBySlotTags AllowTouchingBySlotTags = new();
    internal static readonly AlwaysHearSelectedUsers AlwaysHearSelectedUsers = new();
    internal static readonly DenyGrabbingBySlotTags DenyGrabbingBySlotTags = new();
    internal static readonly DenyHearingBySlotTags DenyHearingBySlotTags = new();
    internal static readonly DenyTouchingBySlotTags DenyTouchingBySlotTags = new();
    internal static readonly DisableNameplates DisableNameplates = new();
    internal static readonly DisableNotifications DisableNotifications = new();
    internal static readonly DisableVrTrackers DisableVrTrackers = new();
    internal static readonly EnforceSelectiveHearing EnforceSelectiveHearing = new();
    internal static readonly EnforceWhispering EnforceWhispering = new();
    internal static readonly HearingVolume HearingVolume = new();
    internal static readonly HideContextMenuItems HideContextMenuItems = new();
    internal static readonly HideDashScreens HideDashScreens = new();
    internal static readonly HideOthersContextMenus HideOthersContextMenus = new();
    internal static readonly HideUserAvatars HideUserAvatars = new();
    internal static readonly MaximumHearingDistance MaximumHearingDistance = new();
    internal static readonly MaximumLaserDistance MaximumLaserDistance = new();
    internal static readonly MovementSpeedMultiplier MovementSpeedMultiplier = new();
    internal static readonly PreventChangeLocomotion PreventChangeLocomotion = new();
    internal static readonly PreventClimbing PreventClimbing = new();
    internal static readonly PreventCrouching PreventCrouching = new();
    internal static readonly PreventEditMode PreventEditMode = new();
    internal static readonly PreventEmergencyRespawning PreventEmergencyRespawning = new();
    internal static readonly PreventEquippingAvatar PreventEquippingAvatar = new();
    internal static readonly PreventGrabbing PreventGrabbing = new();
    internal static readonly PreventHearing PreventHearing = new();
    internal static readonly PreventHearingOfSounds PreventHearingOfSounds = new();
    internal static readonly PreventHearingOfUsers PreventHearingOfUsers = new();
    internal static readonly PreventInviteContact PreventInviteContact = new();
    internal static readonly PreventJumping PreventJumping = new();
    internal static readonly PreventLaserTouch PreventLaserTouch = new();
    internal static readonly PreventLeavingAnchors PreventLeavingAnchors = new();
    internal static readonly PreventMovement PreventMovement = new();
    internal static readonly PreventNonDashUserspaceInteraction PreventNonDashUserspaceInteraction = new();
    internal static readonly PreventOpeningContextMenu PreventOpeningContextMenu = new();
    internal static readonly PreventOpeningDash PreventOpeningDash = new();
    internal static readonly PreventPhysicalTouch PreventPhysicalTouch = new();
    internal static readonly PreventReading PreventReading = new();
    internal static readonly PreventRespawning PreventRespawning = new();
    internal static readonly PreventRunning PreventRunning = new();
    internal static readonly PreventSaveItems PreventSaveItems = new();
    internal static readonly PreventSendingMessages PreventSendingMessages = new();
    internal static readonly PreventSpawnObjects PreventSpawnObjects = new();
    internal static readonly PreventSpeaking PreventSpeaking = new();
    internal static readonly PreventSwitchingWorld PreventSwitchingWorld = new();
    internal static readonly PreventThirdPersonView PreventThirdPersonView = new();
    internal static readonly PreventTurning PreventTurning = new();
    internal static readonly PreventUserScaling PreventUserScaling = new();
    internal static readonly PreventUsingTools PreventUsingTools = new();
    internal static readonly ResetUserScale ResetUserScale = new();
    internal static readonly ShowContextMenuItems ShowContextMenuItems = new();
    internal static readonly ShowDashScreens ShowDashScreens = new();
    internal static readonly ShowUserAvatars ShowUserAvatars = new();
    internal static readonly SpeakingVolume SpeakingVolume = new();
    internal static readonly TrackerMovementSpeed TrackerMovementSpeed = new();

    static Restrictions()
    {
        ResoniteMod.Msg("Initializing restrictions");
        All = typeof(Restrictions)
            .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
            .Where(fieldInfo => typeof(IRestriction).IsAssignableFrom(fieldInfo.FieldType))
            .Select(fieldInfo => fieldInfo.GetValue(null) as IRestriction)
            .Where(t => t != null)
            .OrderBy(restriction => restriction?.Name)
            .ToArray()!;
        ResoniteMod.Msg($"Found {All.Length} restrictions");
        for (var i = 0; i < All.Length; i++)
        {
            All[i].Index = i;
            NameToRestriction.Add(All[i].Name, All[i]);
        }
    }

    internal static int Length => All.Length;

    internal static ILocalRestriction[] CreateLocals(DynamicVariableSpace dynamicVariableSpace,
        IRestriction.IsValid isValid)
    {
        return All.Select(restriction => restriction.CreateLocal(dynamicVariableSpace, isValid)).ToArray();
    }

    internal static bool TryGetByName(string name, out IRestriction restriction)
    {
        return NameToRestriction.TryGetValue(name, out restriction);
    }
}