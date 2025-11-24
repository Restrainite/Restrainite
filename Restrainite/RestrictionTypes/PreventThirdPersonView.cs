using Restrainite.RestrictionTypes.Base;

namespace Restrainite.RestrictionTypes;

internal sealed class PreventThirdPersonView : BaseRestriction
{
    public override string Name => "Prevent Third Person View";

    public override string Description =>
        "Should others be able to prevent you from switching to third-person in desktop mode?";
}