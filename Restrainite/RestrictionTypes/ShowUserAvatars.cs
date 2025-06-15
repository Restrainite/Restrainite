using Restrainite.RestrictionTypes.Base;

namespace Restrainite.RestrictionTypes;

internal class ShowUserAvatars : StringSetRestriction
{
    public override string Name => "Show User Avatars";

    public override string Description =>
        "Should others be able to hide all players except for specific ones? (Inverse of Hide User Avatars.)";
}