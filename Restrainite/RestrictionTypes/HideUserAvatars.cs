using Restrainite.RestrictionTypes.Base;

namespace Restrainite.RestrictionTypes;

internal class HideUserAvatars : StringSetRestriction
{
    public override string Name => "Hide User Avatars";
    public override string Description => "Should others be able to hide specific players?";
}