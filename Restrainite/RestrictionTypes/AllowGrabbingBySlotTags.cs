using Restrainite.RestrictionTypes.Base;

namespace Restrainite.RestrictionTypes;

internal class AllowGrabbingBySlotTags : StringSetRestriction
{
    public override string Name => "Allow Grabbing By Slot Tags";

    public override string Description =>
        "Should others be able to prevent you from grabbing all objects except for those with a specific tag? (Inverse of Deny Grabbing By Slot Tags.)";
}