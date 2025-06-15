using Restrainite.RestrictionTypes.Base;

namespace Restrainite.RestrictionTypes;

internal class PreventSpeaking : BaseRestriction
{
    public override string Name => "Prevent Speaking";
    public override string Description => "Should others be able to mute you?";
}