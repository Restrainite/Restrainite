using Restrainite.RestrictionTypes.Base;

namespace Restrainite.RestrictionTypes;

internal sealed class MaximumHearingDistance : BaseRestriction
{
    public override string Name => "Maximum Hearing Distance";
    public override string Description => "Should others be able to limit how far away you can hear sounds/audio from?";

    public LowestFloatParameter LowestFloat { get; } = new();

    protected override IRestrictionParameter[] InitRestrictionParameters()
    {
        return [LowestFloat];
    }
}