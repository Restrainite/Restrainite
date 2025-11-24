using Restrainite.RestrictionTypes.Base;

namespace Restrainite.RestrictionTypes;

internal sealed class HearingVolume : BaseRestriction
{
    public override string Name => "Hearing Volume";
    public override string Description => "Should others be able to make all sounds/audio quieter for you?";

    public LowestFloatParameter LowestFloat { get; } = new();

    protected override IRestrictionParameter[] InitRestrictionParameters()
    {
        return [LowestFloat];
    }
}