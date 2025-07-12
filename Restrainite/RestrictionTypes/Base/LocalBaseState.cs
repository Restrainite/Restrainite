using System;
using FrooxEngine;
using ResoniteModLoader;

namespace Restrainite.RestrictionTypes.Base;

internal class LocalBaseState<T> : SimpleState<T>, IBaseState
{
    private readonly DynamicVariableChangeListener<T> _changeListener;
    private readonly T _defaultValue;
    private readonly WeakReference<IDynamicVariableSpace> _dynamicVariableSpaceSync;
    private readonly bool _logChanges;
    private readonly IRestriction _restriction;

    internal LocalBaseState(T defaultValue,
        DynamicVariableSpace dynamicVariableSpace,
        IDynamicVariableSpace dynamicVariableSpaceSync,
        IRestriction restriction, bool logChanges) : base(defaultValue)
    {
        _defaultValue = defaultValue;
        _dynamicVariableSpaceSync = new WeakReference<IDynamicVariableSpace>(dynamicVariableSpaceSync);
        _restriction = restriction;
        _logChanges = logChanges;
        _changeListener = new DynamicVariableChangeListener<T>(dynamicVariableSpace, restriction.Name, OnValueChanged);
    }

    public void Destroy()
    {
        _changeListener.Unregister();
    }

    public bool Check(out IDynamicVariableSpace source)
    {
        source = _dynamicVariableSpaceSync.TryGetTarget(out var dynamicVariableSpaceSync)
            ? dynamicVariableSpaceSync
            : DestroyedDynamicVariableSpace.Instance;
        return OnValueChanged(_changeListener.DynamicValue ?? _defaultValue, false);
    }

    private void OnValueChanged(T value)
    {
        OnValueChanged(value ?? _defaultValue, true);
    }

    private bool OnValueChanged(T value, bool triggerEvent)
    {
        if (!_dynamicVariableSpaceSync.TryGetTarget(out var dynamicVariableSpaceSync)) return false;
        var valid = dynamicVariableSpaceSync.IsActiveForLocalUser(_restriction);
        var changed = SetIfChanged(_restriction, valid ? value : _defaultValue);
        if (changed && triggerEvent) _restriction.Update(dynamicVariableSpaceSync);

        if (changed && _logChanges)
            ResoniteMod.Msg(
                $"Local state of {_restriction.Name} changed to {Value} by {dynamicVariableSpaceSync.AsString()}");
        return changed;
    }
}