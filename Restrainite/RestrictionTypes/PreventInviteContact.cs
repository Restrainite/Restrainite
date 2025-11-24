using Restrainite.RestrictionTypes.Base;

namespace Restrainite.RestrictionTypes;

internal sealed class PreventInviteContact : BaseRestriction
{
    public override string Name => "Prevent Invite Contact";

    public override string Description =>
        "Should others be able to prevent you from inviting contacts to your current world?";
}