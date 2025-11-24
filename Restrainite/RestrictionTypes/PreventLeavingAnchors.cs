using Restrainite.RestrictionTypes.Base;

namespace Restrainite.RestrictionTypes;

internal sealed class PreventLeavingAnchors : BaseRestriction
{
    public override string Name => "Prevent Leaving Anchors";
    public override string Description => "Should others be able to prevent you from leaving seats/anchors yourself?";

    public AvatarAnchorParameter AvatarAnchors { get; } = new();

    protected override IRestrictionParameter[] InitRestrictionParameters()
    {
        return [AvatarAnchors];
    }
}