using Restrainite.RestrictionTypes.Base;

namespace Restrainite.RestrictionTypes;

internal class PreventNonDashUserspaceInteraction : BaseRestriction
{
    public override string Name => "Prevent Non Dash Userspace Interaction";

    public override string Description =>
        "Should others be able to prevent you from interacting with anything in userspace besides the dashboard and " +
        "notice popups? (Facet anchors, userspace inspectors, etc. can no longer be interacted with.)";
}