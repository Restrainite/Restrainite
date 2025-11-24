using Restrainite.RestrictionTypes.Base;

namespace Restrainite.RestrictionTypes;

internal sealed class ShowContextMenuItems : BaseRestriction
{
    public override string Name => "Show Context Menu Items";

    public override string Description =>
        "Should others be able to hide all context menu items except for specific ones? (Inverse of Hide Context Menu Items.)";

    public StringSetParameter StringSet { get; } = new();

    protected override IRestrictionParameter[] InitRestrictionParameters()
    {
        return [StringSet];
    }
}