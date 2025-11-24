using Restrainite.RestrictionTypes.Base;

namespace Restrainite.RestrictionTypes;

internal sealed class DenyTouchingBySlotTags : BaseRestriction, ISlotTagRestriction
{
    public override string Name => "Deny Touching By Slot Tags";

    public override string Description =>
        "Should others be able to prevent you from touching objects with a specific tag?";

    public StringSetParameter StringSet { get; } = new();

    protected override IRestrictionParameter[] InitRestrictionParameters()
    {
        return [StringSet];
    }
}