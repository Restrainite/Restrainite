using Restrainite.RestrictionTypes.Base;

namespace Restrainite.RestrictionTypes;

internal class PreventOpeningDash : BaseRestriction
{
    public override string Name => "Prevent Opening Dash";
    public override string Description => "Should others be able to prevent you from opening the dashboard?";
}