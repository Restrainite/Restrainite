using Restrainite.RestrictionTypes.Base;

namespace Restrainite.RestrictionTypes;

internal class PreventHearing : BaseRestriction
{
    public override string Name => "Prevent Hearing";
    public override string Description => "Should others be able to mute all audio sources including voices?";
}