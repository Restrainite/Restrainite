using Restrainite.RestrictionTypes.Base;

namespace Restrainite.RestrictionTypes;

internal sealed class TrackerMovementSpeed : BaseRestriction
{
    public override string Name => "Tracker Movement Speed";

    public override string Description =>
        "Should others be able to slow down or freeze your full-body trackers? (Including VR controllers.)";

    public LowestFloatParameter LowestFloat { get; } = new();

    protected override IRestrictionParameter[] InitRestrictionParameters()
    {
        return [LowestFloat];
    }
}