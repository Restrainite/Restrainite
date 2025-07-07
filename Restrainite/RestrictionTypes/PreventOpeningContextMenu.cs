using Restrainite.RestrictionTypes.Base;

namespace Restrainite.RestrictionTypes;

internal class PreventOpeningContextMenu : BaseRestriction
{
    public override string Name => "Prevent Opening Context Menu";
    public override string Description => "Should others be able to prevent you from opening your context menu?";
}