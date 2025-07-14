using Restrainite.RestrictionTypes.Base;

namespace Restrainite.RestrictionTypes;

internal class EnforceSelectiveHearing : BaseRestriction
{
    public override string Name => "Enforce Selective Hearing";

    public override string Description =>
        "[Deprecated, use Always Hear Selected Users] Should others be able to limit the voices you can hear to specific players?";

    public override bool IsDeprecated => true;

    public StringSetParameter StringSet { get; } = new();

    protected override IRestrictionParameter[] InitRestrictionParameters()
    {
        return [StringSet];
    }
}