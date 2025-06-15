using Restrainite.RestrictionTypes.Base;

namespace Restrainite.RestrictionTypes;

internal class PreventMovement : BaseRestriction
{
    public override string Name => "Prevent Movement";
    public override string Description => "Should others be able to prevent you from walking/moving by yourself?";
}