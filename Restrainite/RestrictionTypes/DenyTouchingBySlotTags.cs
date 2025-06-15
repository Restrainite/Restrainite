using Restrainite.RestrictionTypes.Base;

namespace Restrainite.RestrictionTypes;

internal class DenyTouchingBySlotTags : StringSetRestriction
{
    public override string Name => "Deny Touching By Slot Tags";

    public override string Description =>
        "Should others be able to prevent you from touching objects with a specific tag?";
}