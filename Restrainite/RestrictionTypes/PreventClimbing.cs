using Restrainite.RestrictionTypes.Base;

namespace Restrainite.RestrictionTypes;

internal class PreventClimbing : BaseRestriction
{
    public override string Name => "Prevent Climbing";

    public override string Description =>
        "Should others be able to prevent you from climbing by grabbing the world or characters?";

    public ChiralityParameter Chirality { get; } = new();

    protected override IRestrictionParameter[] InitRestrictionParameters()
    {
        return [Chirality];
    }
}