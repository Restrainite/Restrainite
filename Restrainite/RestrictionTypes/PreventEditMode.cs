using Restrainite.RestrictionTypes.Base;

namespace Restrainite.RestrictionTypes;

internal class PreventEditMode : BaseRestriction
{
    public override string Name => "Prevent Edit Mode";
    public override string Description => "Should others be able to disable your ability to enable Edit Mode (F2)?";
}