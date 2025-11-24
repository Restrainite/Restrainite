using Restrainite.RestrictionTypes.Base;

namespace Restrainite.RestrictionTypes;

internal sealed class PreventRunning : BaseRestriction
{
    public override string Name => "Prevent Running";

    public override string Description =>
        "Should others be able to prevent you from sprinting/running? (On desktop, this disables the sprint input, and in VR, you can't use both joysticks to move faster.)";
}