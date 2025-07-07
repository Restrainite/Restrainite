using Restrainite.RestrictionTypes.Base;

namespace Restrainite.RestrictionTypes;

internal class MovementSpeedMultiplier : LowestFloatRestriction
{
    public override string Name => "Movement Speed Multiplier";
    public override string Description => "Should others be able to limit your movement speed?";
}