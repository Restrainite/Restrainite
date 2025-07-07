using Restrainite.RestrictionTypes.Base;

namespace Restrainite.RestrictionTypes;

internal class PreventSendingMessages : BaseRestriction
{
    public override string Name => "Prevent Sending Messages";
    public override string Description => "Should others be able to prevent you from sending messages to contacts?";
}