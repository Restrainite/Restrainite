using Restrainite.Enums;

namespace Restrainite.States;

internal class LocalStateBool : LocalState<bool, bool>
{
    internal LocalStateBool(IDynamicVariableSpaceWrapper dynamicVariableSpaceWrapper) :
        base(dynamicVariableSpaceWrapper, false, PreventionTypes.List)
    {
        OnUpdateGlobal += RestrainiteMod.BoolState.Update;
        OnUpdateGlobal += RestrainiteMod.LowestFloatState.Update;
        OnUpdateGlobal += RestrainiteMod.StringSetState.Update;
    }

    protected override bool Transform(bool value)
    {
        return value;
    }
}