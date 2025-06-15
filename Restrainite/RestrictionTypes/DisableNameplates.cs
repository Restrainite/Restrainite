using Restrainite.RestrictionTypes.Base;

namespace Restrainite.RestrictionTypes;

internal class DisableNameplates : BaseRestriction
{
    public override string Name => "Disable Nameplates";
    public override string Description => "Should others be able to disable avatar nameplates for you?";
}