using System;
using FrooxEngine;

namespace Restrainite.RestrictionTypes.Base;

internal class LocalBaseRestriction : ILocalRestriction
{
    private readonly SimpleState<bool> _state = new(false);
    private DynamicVariableChangeListener<bool>? _changeListener;
    private IRestriction.IsValid? _isValid;
    protected IRestriction? Restriction;

    public virtual void Destroy()
    {
        OnDestroy.SafeInvoke();
        if (_changeListener == null) return;
        _changeListener.Unregister(OnValueChanged);
        _changeListener = null;
        _isValid = null;
    }

    public virtual void Check()
    {
        OnValueChanged(_changeListener?.DynamicValue ?? false);
    }

    public event Action? OnChanged;
    public event Action? OnDestroy;

    public bool IsActive()
    {
        return IsValid() && _state.Value;
    }

    public virtual void Register(DynamicVariableSpace dynamicVariableSpace, IRestriction.IsValid isValid,
        IRestriction restriction)
    {
        _isValid = isValid;
        Restriction = restriction;
        _changeListener = new DynamicVariableChangeListener<bool>(dynamicVariableSpace, restriction.Name);
        _changeListener.Register(OnValueChanged);
        OnValueChanged(_changeListener.DynamicValue);
    }

    private bool IsValid()
    {
        return _isValid != null && Restriction != null && _isValid(Restriction);
    }

    private void OnValueChanged(bool value)
    {
        if (Restriction == null) return;
        var changed = _state.SetIfChanged(Restriction, IsValid() && value);
        if (!changed) return;
        _changeListener?.LogChange("Local bool", Restriction, _state.Value);
        OnStateChanged();
    }

    protected virtual void OnStateChanged()
    {
        OnChanged.SafeInvoke();
    }
}