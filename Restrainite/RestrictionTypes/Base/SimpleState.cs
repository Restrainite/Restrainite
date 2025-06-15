using System;

namespace Restrainite.RestrictionTypes.Base;

internal class SimpleState<T>(T defaultValue) where T : IEquatable<T>
{
    internal T Value { get; private set; } = defaultValue;

    internal event Action<IRestriction, T>? OnStateChanged;

    internal bool SetIfChanged(IRestriction restriction, T newValue)
    {
        if (Value.Equals(newValue)) return false;
        Value = newValue;
        OnStateChanged.SafeInvoke(restriction, Value);
        return true;
    }
}