using Restrainite.RestrictionTypes.Base;

namespace Restrainite.RestrictionTypes;

internal sealed class ResetUserScale : BaseRestriction
{
    public override string Name => "Reset User Scale";
    public override string Description => "Should others be able to force you back to your default avatar scale?";
}