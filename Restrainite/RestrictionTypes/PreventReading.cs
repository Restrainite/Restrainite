using Restrainite.RestrictionTypes.Base;

namespace Restrainite.RestrictionTypes;

internal class PreventReading : BaseRestriction
{
    public override string Name => "Prevent Reading";

    public override string Description =>
        "Should others be able to scramble all text on screen, making you unable to read?";
}