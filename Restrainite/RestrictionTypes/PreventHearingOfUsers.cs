using Restrainite.RestrictionTypes.Base;

namespace Restrainite.RestrictionTypes;

internal sealed class PreventHearingOfUsers : BaseRestriction
{
    public override string Name => "Prevent Hearing Of Users";
    public override string Description => "Should others be able to mute all user voices for you?";
}