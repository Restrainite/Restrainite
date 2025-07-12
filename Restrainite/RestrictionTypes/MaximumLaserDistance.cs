using Restrainite.RestrictionTypes.Base;

namespace Restrainite.RestrictionTypes;

internal class MaximumLaserDistance : BaseRestriction
{
    public override string Name => "Maximum Laser Distance";

    public override string Description =>
        "Should others be able to limit the distance on objects you can interact with? (Limiting how far your interaction laser reaches.)";

    public LowestFloatParameter LowestFloat { get; } = new();

    protected override IRestrictionParameter[] InitRestrictionParameters()
    {
        return [LowestFloat];
    }
}