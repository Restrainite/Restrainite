using FrooxEngine;
using Restrainite.RestrictionTypes.Base;
using SkyFrost.Base;

namespace Restrainite;

internal static class SetBusyStatus
{
    private static bool _previousState;
    private static OnlineStatus _previousOnlineStatus = OnlineStatus.Offline;

    internal static void Initialize()
    {
        Restrictions.PreventOpeningDash.OnChanged += OnChanged;
        Restrictions.DisableNotifications.OnChanged += OnChanged;
        Restrictions.HideDashScreens.OnChanged += OnChanged;
        Restrictions.ShowDashScreens.OnChanged += OnChanged;
        Restrictions.PreventReading.OnChanged += OnChanged;
        Restrictions.PreventSendingMessages.OnChanged += OnChanged;
    }

    private static void OnChanged(IRestriction restriction)
    {
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
        if (Restrictions.PreventOpeningDash.IsRestricted ||
            Restrictions.DisableNotifications.IsRestricted ||
            Restrictions.PreventReading.IsRestricted ||
            Restrictions.PreventSendingMessages.IsRestricted)
            return true;

        if (Restrictions.HideDashScreens.IsRestricted &&
            Restrictions.HideDashScreens.SetContains("Dash.Screens.Contacts"))
            return true;

        if (Restrictions.ShowDashScreens.IsRestricted &&
            !Restrictions.ShowDashScreens.SetContains("Dash.Screens.Contacts"))
            return true;
        return false;
    }
}