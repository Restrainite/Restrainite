using System;
using System.Collections.Generic;

namespace Restrainite.RestrictionTypes.Base;

internal class SimpleState<T>(T defaultValue)
{
    internal T Value { get; private set; } = defaultValue;

    internal event Action<IRestriction, T>? OnStateChanged;

    internal bool SetIfChanged(IRestriction restriction,
        T newValue)
    {
        if (EqualityComparer<T>.Default.Equals(Value, newValue)) return false;
        Value = newValue;
        OnStateChanged.SafeInvoke(restriction, Value);
        return true;
    }
}