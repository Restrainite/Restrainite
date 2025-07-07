using Restrainite.RestrictionTypes.Base;

namespace Restrainite.RestrictionTypes;

internal class EnforceWhispering : BaseRestriction
{
    public override string Name => "Enforce Whispering";

    public override string Description =>
        "Should others be able to forcefully make you whisper? (You can still mute yourself.)";
}