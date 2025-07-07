using Restrainite.RestrictionTypes.Base;

namespace Restrainite.RestrictionTypes;

internal class PreventCrouching : BaseRestriction
{
    public override string Name => "Prevent Crouching";
    public override string Description => "Should others be able to prevent you from crouching in desktop mode?";
}