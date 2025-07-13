using Restrainite.RestrictionTypes.Base;

namespace Restrainite.RestrictionTypes;

internal class EnforceWhispering : BaseRestriction
{
    public override string Name => "Enforce Whispering";

    public override string Description =>
        "[Deprecated, use Maximum Voice Mode] Should others be able to forcefully make you whisper? (You can still mute yourself.)";

    public override bool IsDeprecated => true;
}