using System.Collections.Generic;
using Restrainite.Enums;

namespace Restrainite.States;

internal class GlobalStateBool() : GlobalState<bool>(false)
{
    protected override bool Calculate(PreventionType preventionType, ICollection<IDynamicVariableSpaceWrapper> spaces)
    {
        return spaces.Count > 0;
    }

    protected override bool IsCorrectType(PreventionType preventionType)
    {
        return true;
    }
}