using Restrainite.RestrictionTypes.Base;

namespace Restrainite.RestrictionTypes;

internal class ShowDashScreens : StringSetRestriction
{
    public override string Name => "Show Dash Screens";

    public override string Description =>
        "Should others be able to hide all dashboard screens except for specific ones? (Inverse of Hide Dash Screens, excludes Exit screen.)";
}