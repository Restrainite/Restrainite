using Restrainite.RestrictionTypes.Base;

namespace Restrainite.RestrictionTypes;

internal sealed class HideOthersContextMenus : BaseRestriction
{
    public override string Name => "Hide Others Context Menus";
    public override string Description => "Should others be able to prevent you from seeing other users context menus.";
}