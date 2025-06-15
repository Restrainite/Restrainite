using Restrainite.RestrictionTypes.Base;

namespace Restrainite.RestrictionTypes;

internal class PreventPhysicalTouch : BaseRestriction
{
    public override string Name => "Prevent Physical Touch";
    public override string Description => "Should others be able to prevent you from any physically-based interaction?";
}