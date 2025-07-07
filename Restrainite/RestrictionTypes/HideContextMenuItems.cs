using Restrainite.RestrictionTypes.Base;

namespace Restrainite.RestrictionTypes;

internal class HideContextMenuItems : StringSetRestriction
{
    public override string Name => "Hide Context Menu Items";
    public override string Description => "Should others be able to hide specific context menu items?";
}