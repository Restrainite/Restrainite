using Restrainite.RestrictionTypes.Base;

namespace Restrainite.RestrictionTypes;

internal class AllowTouchingBySlotTags : BaseRestriction, ISlotTagRestriction
{
    public override string Name => "Allow Touching By Slot Tags";

    public override string Description =>
        "Should others be able to prevent you from touching all objects except for those with a specific tag? (Inverse of Deny Touching By Slot Tags.)";

    public StringSetParameter StringSet { get; } = new();

    protected override IRestrictionParameter[] InitRestrictionParameters()
    {
        return [StringSet];
    }
}