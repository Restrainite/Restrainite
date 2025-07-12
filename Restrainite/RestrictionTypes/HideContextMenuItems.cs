using Restrainite.RestrictionTypes.Base;

namespace Restrainite.RestrictionTypes;

internal class HideContextMenuItems : BaseRestriction
{
    public override string Name => "Hide Context Menu Items";
    public override string Description => "Should others be able to hide specific context menu items?";

    public StringSetParameter StringSet { get; } = new();

    protected override IRestrictionParameter[] InitRestrictionParameters()
    {
        return [StringSet];
    }
}