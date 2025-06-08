using FrooxEngine;
using Restrainite.Enums;
using SkyFrost.Base;

namespace Restrainite;

internal static class SetBusyStatus
{
    private static bool _previousState;
    private static OnlineStatus _previousOnlineStatus = OnlineStatus.Offline;

    internal static void Initialize()
    {
        RestrainiteMod.BoolState.OnChanged += OnChange;
    }

    private static void OnChange(PreventionType preventionType, bool value)
    {
        if (preventionType != PreventionType.PreventOpeningDash &&
            preventionType != PreventionType.DisableNotifications &&
            preventionType != PreventionType.HideDashScreens &&
            preventionType != PreventionType.ShowDashScreens &&
            preventionType != PreventionType.PreventReading &&
            preventionType != PreventionType.PreventSendingMessages) return;

        if (!RestrainiteMod.Configuration.SetBusyStatus) return;

        var currentValue = Engine.Current.Cloud.Status.OnlineStatus;
        if (currentValue is OnlineStatus.Offline or OnlineStatus.Invisible) return;
        var state = GetExpectedState();
        if (_previousState == state) return;

        _previousState = state;
        if (state)
        {
            _previousOnlineStatus = currentValue;
            Engine.Current.Cloud.Status.OnlineStatus = OnlineStatus.Busy;
        }
        else if (currentValue == OnlineStatus.Busy)
        {
            Engine.Current.Cloud.Status.OnlineStatus = _previousOnlineStatus;
        }
    }

    private static bool GetExpectedState()
    {
        if (RestrainiteMod.IsRestricted(PreventionType.PreventOpeningDash) ||
            RestrainiteMod.IsRestricted(PreventionType.DisableNotifications) ||
            RestrainiteMod.IsRestricted(PreventionType.PreventReading) ||
            RestrainiteMod.IsRestricted(PreventionType.PreventSendingMessages))
            return true;

        if (RestrainiteMod.IsRestricted(PreventionType.HideDashScreens) &&
            RestrainiteMod.GetStringSet(PreventionType.HideDashScreens).Contains("Dash.Screens.Contacts"))
            return true;

        if (RestrainiteMod.IsRestricted(PreventionType.ShowDashScreens) &&
            !RestrainiteMod.GetStringSet(PreventionType.ShowDashScreens).Contains("Dash.Screens.Contacts"))
            return true;
        return false;
    }
}