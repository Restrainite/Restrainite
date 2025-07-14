using Restrainite.RestrictionTypes.Base;

namespace Restrainite.RestrictionTypes;

internal class HideUserAvatars : BaseRestriction
{
    public override string Name => "Hide User Avatars";
    public override string Description => "Should others be able to hide specific players?";

    public StringSetParameter StringSet { get; } = new();

    protected override IRestrictionParameter[] InitRestrictionParameters()
    {
        return [StringSet];
    }
}