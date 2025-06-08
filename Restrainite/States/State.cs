using System.Collections.Generic;
using ResoniteModLoader;
using Restrainite.Enums;

namespace Restrainite.States;

internal abstract class State<TInternal>(TInternal defaultValue) where TInternal : notnull
{
    private readonly TInternal[] _stateValues = PreventionTypes.CreateDefaultArray(defaultValue);
    protected readonly TInternal DefaultValue = defaultValue;

    protected bool SetIfChanged(PreventionType preventionType, TInternal newValue)
    {
        var currentValue = _stateValues[(int)preventionType];
        if (EqualityComparer<TInternal>.Default.Equals(currentValue, newValue)) return false;
        _stateValues[(int)preventionType] = newValue;
        return true;
    }

    internal TInternal Get(PreventionType preventionType)
    {
        return _stateValues[(int)preventionType];
    }

    protected void LogChange(string typeName, PreventionType preventionType, TInternal value,
        IDynamicVariableSpaceWrapper source)
    {
        ResoniteMod.Msg($"{typeName} of {preventionType.ToExpandedString()} changed to '{value}'. ({source})");
    }
}