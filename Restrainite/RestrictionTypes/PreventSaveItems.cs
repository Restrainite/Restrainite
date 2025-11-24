using Restrainite.RestrictionTypes.Base;

namespace Restrainite.RestrictionTypes;

internal sealed class PreventSaveItems : BaseRestriction
{
    public override string Name => "Prevent Save Items";
    public override string Description => "Should others be able to prevent you from saving items to your inventory?";
}