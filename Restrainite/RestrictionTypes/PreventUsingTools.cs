using Restrainite.RestrictionTypes.Base;

namespace Restrainite.RestrictionTypes;

internal class PreventUsingTools : BaseRestriction
{
    public override string Name => "Prevent Using Tools";
    public override string Description => "Should others be able to prevent you from equipping tools?";

    public ChiralityParameter Chirality { get; } = new();

    protected override IRestrictionParameter[] InitRestrictionParameters()
    {
        return [Chirality];
    }
}