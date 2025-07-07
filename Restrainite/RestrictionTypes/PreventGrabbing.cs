using Restrainite.RestrictionTypes.Base;

namespace Restrainite.RestrictionTypes;

internal class PreventGrabbing : BaseRestriction
{
    public override string Name => "Prevent Grabbing";
    public override string Description => "Should others be able to prevent you from grabbing objects?";
}