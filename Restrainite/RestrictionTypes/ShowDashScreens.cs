using Restrainite.RestrictionTypes.Base;

namespace Restrainite.RestrictionTypes;

internal sealed class ShowDashScreens : BaseRestriction
{
    public override string Name => "Show Dash Screens";

    public override string Description =>
        "Should others be able to hide all dashboard screens except for specific ones? (Inverse of Hide Dash Screens, excludes Exit screen.)";

    public StringSetParameter StringSet { get; } = new();

    protected override IRestrictionParameter[] InitRestrictionParameters()
    {
        return [StringSet];
    }
}