using Restrainite.RestrictionTypes.Base;

namespace Restrainite.RestrictionTypes;

internal class PreventJumping : BaseRestriction
{
    public override string Name => "Prevent Jumping";

    public override string Description =>
        "Should others be able to prevent you from jumping? (Does not prevent exiting anchors.)";
}