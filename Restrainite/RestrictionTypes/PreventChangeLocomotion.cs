using Restrainite.RestrictionTypes.Base;

namespace Restrainite.RestrictionTypes;

internal sealed class PreventChangeLocomotion : BaseRestriction
{
    public override string Name => "Prevent Change Locomotion";
    public override string Description => "Should others be able to prevent you from changing your locomotion mode?";
}