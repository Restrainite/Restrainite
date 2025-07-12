using Restrainite.RestrictionTypes.Base;

namespace Restrainite.RestrictionTypes;

internal class DenyGrabbingBySlotTags : BaseRestriction, ISlotTagRestriction
{
    public override string Name => "Deny Grabbing By Slot Tags";

    public override string Description =>
        "Should others be able to prevent you from grabbing objects with a specific tag?";

    public StringSetParameter StringSet { get; } = new();

    protected override IRestrictionParameter[] InitRestrictionParameters()
    {
        return [StringSet];
    }
}