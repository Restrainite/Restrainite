using Restrainite.RestrictionTypes.Base;

namespace Restrainite.RestrictionTypes;

internal sealed class HideDashScreens : BaseRestriction
{
    public override string Name => "Hide Dash Screens";

    public override string Description =>
        "Should others be able to hide specific dashboard screens? (Excluding the Exit screen.)";

    public StringSetParameter StringSet { get; } = new();

    protected override IRestrictionParameter[] InitRestrictionParameters()
    {
        return [StringSet];
    }
}