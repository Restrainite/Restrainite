using Restrainite.RestrictionTypes.Base;

namespace Restrainite.RestrictionTypes;

internal sealed class DisableNotifications : BaseRestriction
{
    public override string Name => "Disable Notifications";
    public override string Description => "Should others be able to disable your notifications?";
}