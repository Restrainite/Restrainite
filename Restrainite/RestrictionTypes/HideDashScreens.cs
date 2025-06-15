using Restrainite.RestrictionTypes.Base;

namespace Restrainite.RestrictionTypes;

internal class HideDashScreens : StringSetRestriction
{
    public override string Name => "Hide Dash Screens";

    public override string Description =>
        "Should others be able to hide specific dashboard screens? (Excluding the Exit screen.)";
}