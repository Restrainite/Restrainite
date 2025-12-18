using Restrainite.RestrictionTypes.Base;

namespace Restrainite.RestrictionTypes;

internal sealed class MaximumLaserDistance : BaseRestriction
{
    public override string Name => "Maximum Laser Distance";

    public override string Description =>
        "Should others be able to limit the distance on objects you can interact with? (Limiting how far your interaction laser reaches.)";

    public LowestFloatParameter LowestFloat { get; } = new(0.0f);

    public ChiralityParameter Chirality { get; } = new();

    protected override IRestrictionParameter[] InitRestrictionParameters()
    {
        return [LowestFloat, Chirality];
    }
}