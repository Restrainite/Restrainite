using Restrainite.RestrictionTypes.Base;

namespace Restrainite.RestrictionTypes;

internal sealed class PreventEmergencyRespawning : BaseRestriction
{
    public override string Name => "Prevent Emergency Respawning";

    public override string Description =>
        "Should others be able to prevent you from using the emergency respawn gesture?";
}