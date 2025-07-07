using System;

namespace Restrainite.RestrictionTypes.Base;

internal class SimpleState<T>(T defaultValue) where T : IEquatable<T>
{
    internal T Value { get; private set; } = defaultValue;

    internal event Action<IRestriction, T, IDynamicVariableSpace>? OnStateChanged;

    internal bool SetIfChanged(IRestriction restriction,
        T newValue,
        IDynamicVariableSpace stateChangeSource,
        bool triggerEvent = true)
    {
        if (Value.Equals(newValue)) return false;
        Value = newValue;
        if (triggerEvent) OnStateChanged.SafeInvoke(restriction, Value, stateChangeSource);
        return true;
    }
}