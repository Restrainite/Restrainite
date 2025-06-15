using Restrainite.RestrictionTypes.Base;

namespace Restrainite.RestrictionTypes;

internal class PreventLaserTouch : BaseRestriction
{
    public override string Name => "Prevent Laser Touch";
    public override string Description => "Should others be able to prevent you from any laser-based interaction?";
}