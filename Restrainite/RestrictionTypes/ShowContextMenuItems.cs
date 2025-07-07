using Restrainite.RestrictionTypes.Base;

namespace Restrainite.RestrictionTypes;

internal class ShowContextMenuItems : StringSetRestriction
{
    public override string Name => "Show Context Menu Items";

    public override string Description =>
        "Should others be able to hide all context menu items except for specific ones? (Inverse of Hide Context Menu Items.)";
}