using Restrainite.RestrictionTypes.Base;

namespace Restrainite.RestrictionTypes;

internal class PreventSwitchingWorld : BaseRestriction
{
    public override string Name => "Prevent Switching World";

    public override string Description =>
        "Should others be able to prevent you from starting a new world, joining another session, leaving the current world, or changing focus?";
}