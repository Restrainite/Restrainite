using System;
using FrooxEngine;

namespace Restrainite.RestrictionTypes.Base;

internal class LocalBaseState<T>(T defaultValue) :
    SimpleState<T>(defaultValue) where T : IEquatable<T>
{
    private readonly T _defaultValue = defaultValue;
    private readonly string _typeName = "Local " + typeof(T);
    private DynamicVariableChangeListener<T>? _changeListener;
    private IDynamicVariableSpaceSync? _dynamicVariableSpaceSync;
    private IRestriction? _restriction;

    public void Register(DynamicVariableSpace dynamicVariableSpace, IDynamicVariableSpaceSync dynamicVariableSpaceSync,
        IRestriction restriction)
    {
        _dynamicVariableSpaceSync = dynamicVariableSpaceSync;
        _restriction = restriction;
        _changeListener = new DynamicVariableChangeListener<T>(dynamicVariableSpace, restriction.Name);
        _changeListener.Register(OnValueChanged);
        Check();
    }

    public void Destroy()
    {
        if (_changeListener == null) return;
        _changeListener.Unregister(OnValueChanged);
        _changeListener = null;
        _restriction = null;
        _dynamicVariableSpaceSync = null;
    }

    public void Check(bool triggerEvent = true)
    {
        if (_changeListener == null) return;
        OnValueChanged(_changeListener.DynamicValue ?? _defaultValue, triggerEvent);
    }

    private void OnValueChanged(T value)
    {
        OnValueChanged(value, true);
    }

    private void OnValueChanged(T value, bool triggerEvent)
    {
        if (_restriction == null) return;
        var valid = _dynamicVariableSpaceSync != null && _dynamicVariableSpaceSync.IsActiveForLocalUser(_restriction);
        var changed = SetIfChanged(_restriction, valid ? value ?? _defaultValue : _defaultValue, triggerEvent);
        if (!changed) return;
        _changeListener?.LogChange(_typeName, _restriction, Value);
    }
}