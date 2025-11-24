using Restrainite.RestrictionTypes.Base;

namespace Restrainite.RestrictionTypes;

internal sealed class PreventUserScaling : BaseRestriction
{
    public override string Name => "Prevent User Scaling";
    public override string Description => "Should others be able to prevent you from rescaling yourself?";
}