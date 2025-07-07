using Restrainite.RestrictionTypes.Base;

namespace Restrainite.RestrictionTypes;

internal class MaximumHearingDistance : LowestFloatRestriction
{
    public override string Name => "Maximum Hearing Distance";
    public override string Description => "Should others be able to limit how far away you can hear sounds/audio from?";
}