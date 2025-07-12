using Restrainite.RestrictionTypes.Base;

namespace Restrainite.RestrictionTypes;

internal class AllowHearingBySlotTags : BaseRestriction, ISlotTagRestriction
{
    public override string Name => "Allow Hearing By Slot Tags";

    public override string Description =>
        "Should others be able to prevent you from hearing all audio sources except for those with a specific tag? (Inverse of Deny Hearing By Slot Tags.)";

    public StringSetParameter StringSet { get; } = new();

    protected override IRestrictionParameter[] InitRestrictionParameters()
    {
        return [StringSet];
    }
}