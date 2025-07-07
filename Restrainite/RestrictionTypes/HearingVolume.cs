using Restrainite.RestrictionTypes.Base;

namespace Restrainite.RestrictionTypes;

internal class HearingVolume : LowestFloatRestriction
{
    public override string Name => "Hearing Volume";
    public override string Description => "Should others be able to make all sounds/audio quieter for you?";
}