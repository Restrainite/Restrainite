using Restrainite.RestrictionTypes.Base;

namespace Restrainite.RestrictionTypes;

internal sealed class PreventSpawnObjects : BaseRestriction
{
    public override string Name => "Prevent Spawn Objects";
    public override string Description => "Should others be able to prevent you from spawning objects?";
}