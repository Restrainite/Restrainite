using Restrainite.RestrictionTypes.Base;

namespace Restrainite.RestrictionTypes;

internal sealed class PreventHearingOfSounds : BaseRestriction
{
    public override string Name => "Prevent Hearing Of Sounds";
    public override string Description => "Should others be able to mute all sounds except other users voices?";
}