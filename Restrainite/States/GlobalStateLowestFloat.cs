using System.Collections.Generic;
using Restrainite.Enums;

namespace Restrainite.States;

internal class GlobalStateLowestFloat() : GlobalState<float>(float.NaN)
{
    protected override float Calculate(PreventionType preventionType, ICollection<IDynamicVariableSpaceWrapper> spaces)
    {
        var globalState = float.NaN;

        foreach (var space in spaces)
        {
            var local = space.GetLowestFloatState(preventionType);
            if (float.IsNaN(local)) continue;
            if (float.IsNaN(globalState) || local < globalState) globalState = local;
        }

        return globalState;
    }

    protected override bool IsCorrectType(PreventionType preventionType)
    {
        return preventionType.IsFloatType();
    }
}