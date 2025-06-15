using Restrainite.RestrictionTypes.Base;

namespace Restrainite.RestrictionTypes;

internal class DisableVrTrackers : BaseRestriction
{
    public override string Name => "Disable Vr Trackers";

    public override string Description =>
        "Should others be able to disable your VR controllers and full-body trackers?";
}