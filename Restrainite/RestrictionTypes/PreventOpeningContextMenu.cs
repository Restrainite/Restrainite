using Restrainite.RestrictionTypes.Base;

namespace Restrainite.RestrictionTypes;

internal sealed class PreventOpeningContextMenu : BaseRestriction
{
    public override string Name => "Prevent Opening Context Menu";
    public override string Description => "Should others be able to prevent you from opening your context menu?";

    public ChiralityParameter Chirality { get; } = new();

    protected override IRestrictionParameter[] InitRestrictionParameters()
    {
        return [Chirality];
    }
}