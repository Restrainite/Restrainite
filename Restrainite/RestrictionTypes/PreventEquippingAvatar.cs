using Restrainite.RestrictionTypes.Base;

namespace Restrainite.RestrictionTypes;

internal sealed class PreventEquippingAvatar : BaseRestriction
{
    public override string Name => "Prevent Equipping Avatar";
    public override string Description => "Should others be able to prevent you from equipping avatars?";
}