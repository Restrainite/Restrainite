using Restrainite.RestrictionTypes.Base;

namespace Restrainite.RestrictionTypes;

internal class PreventGrabbing : BaseRestriction
{
    public override string Name => "Prevent Grabbing";
    public override string Description => "Should others be able to prevent you from grabbing objects?";

    public ChiralityParameter Chirality { get; } = new();

    protected override IRestrictionParameter[] InitRestrictionParameters()
    {
        return [Chirality];
    }
}