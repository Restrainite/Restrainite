using System.Collections.Generic;
using System.Linq;
using Restrainite.Enums;

namespace Restrainite.States;

internal class LocalStateLowestFloat : LocalState<float, float>
{
    private static readonly IEnumerable<PreventionType> ValidTypes =
        PreventionTypes.List.Where(type => type.IsFloatType()).ToArray();

    internal LocalStateLowestFloat(IDynamicVariableSpaceWrapper dynamicVariableSpaceWrapper) :
        base(dynamicVariableSpaceWrapper, float.NaN, ValidTypes)
    {
        OnUpdateGlobal += RestrainiteMod.LowestFloatState.Update;
    }

    protected override float Transform(float value)
    {
        return value;
    }
}