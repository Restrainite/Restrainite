using Restrainite.RestrictionTypes.Base;

namespace Restrainite.RestrictionTypes;

internal class PreventRespawning : BaseRestriction
{
    public override string Name => "Prevent Respawning";

    public override string Description =>
        "Should others be able to prevent you from respawning? (Except for emergency respawning.)";
}