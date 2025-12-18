using Restrainite.RestrictionTypes.Base;

namespace Restrainite.RestrictionTypes;

internal sealed class MovementSpeedMultiplier : BaseRestriction
{
    public override string Name => "Movement Speed Multiplier";
    public override string Description => "Should others be able to limit your movement speed?";

    public LowestFloatParameter LowestFloat { get; } = new(0.0f, 1.0f);

    protected override IRestrictionParameter[] InitRestrictionParameters()
    {
        return [LowestFloat];
    }
}