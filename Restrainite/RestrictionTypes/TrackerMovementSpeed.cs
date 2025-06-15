using Restrainite.RestrictionTypes.Base;

namespace Restrainite.RestrictionTypes;

internal class TrackerMovementSpeed : LowestFloatRestriction
{
    public override string Name => "Tracker Movement Speed";

    public override string Description =>
        "Should others be able to slow down or freeze your full-body trackers? (Including VR controllers.)";
}