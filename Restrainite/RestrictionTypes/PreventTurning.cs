using Restrainite.RestrictionTypes.Base;

namespace Restrainite.RestrictionTypes;

internal class PreventTurning : BaseRestriction
{
    public override string Name => "Prevent Turning";

    public override string Description =>
        "Should others be able to prevent you from turning your body? (You can still look around in VR.)";
}