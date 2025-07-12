using Restrainite.RestrictionTypes.Base;

namespace Restrainite.RestrictionTypes;

internal class AlwaysHearSelectedUsers : BaseRestriction
{
    public override string Name => "Always Hear Selected Users";

    public override string Description =>
        "Should others be able to control, that you can hear selected users, even if other restrictions forbid it.";

    public StringSetParameter StringSet { get; } = new();

    protected override IRestrictionParameter[] InitRestrictionParameters()
    {
        return [StringSet];
    }
}