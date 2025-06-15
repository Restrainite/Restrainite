using Restrainite.RestrictionTypes.Base;

namespace Restrainite.RestrictionTypes;

internal class DenyHearingBySlotTags : StringSetRestriction
{
    public override string Name => "Deny Hearing By Slot Tags";

    public override string Description =>
        "Should others be able to prevent you from hearing audio sources with a specific tag?";
}