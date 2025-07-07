using Restrainite.RestrictionTypes.Base;

namespace Restrainite.RestrictionTypes;

internal class SpeakingVolume : LowestFloatRestriction
{
    public override string Name => "Speaking Volume";
    public override string Description => "Should others be able to make your voice quieter to everyone else?";
}