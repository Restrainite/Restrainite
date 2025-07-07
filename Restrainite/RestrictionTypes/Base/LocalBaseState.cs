using System;
using FrooxEngine;

namespace Restrainite.RestrictionTypes.Base;

internal class LocalBaseState<T> :
    SimpleState<T> where T : IEquatable<T>
{
    private readonly DynamicVariableChangeListener<T> _changeListener;
    private readonly T _defaultValue;
    private readonly WeakReference<IDynamicVariableSpace> _dynamicVariableSpaceSync;
    private readonly IRestriction _restriction;

    internal LocalBaseState(T defaultValue,
        DynamicVariableSpace dynamicVariableSpace,
        IDynamicVariableSpace dynamicVariableSpaceSync,
        IRestriction restriction) : base(defaultValue)
    {
        _defaultValue = defaultValue;
        _dynamicVariableSpaceSync = new WeakReference<IDynamicVariableSpace>(dynamicVariableSpaceSync);
        _restriction = restriction;
        _changeListener = new DynamicVariableChangeListener<T>(dynamicVariableSpace, restriction.Name, OnValueChanged);
    }

    public void Destroy()
    {
        _changeListener.Unregister();
    }

    public void Check(bool triggerEvent = true)
    {
        OnValueChanged(_changeListener.DynamicValue ?? _defaultValue, triggerEvent);
    }

    private void OnValueChanged(T value)
    {
        OnValueChanged(value ?? _defaultValue, true);
    }

    private void OnValueChanged(T value, bool triggerEvent)
    {
        if (!_dynamicVariableSpaceSync.TryGetTarget(out var dynamicVariableSpaceSync)) return;
        var valid = dynamicVariableSpaceSync.IsActiveForLocalUser(_restriction);
        SetIfChanged(_restriction,
            valid ? value : _defaultValue,
            dynamicVariableSpaceSync, triggerEvent);
    }
}