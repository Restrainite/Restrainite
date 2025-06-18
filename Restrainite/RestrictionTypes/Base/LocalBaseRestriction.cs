using System;
using FrooxEngine;

namespace Restrainite.RestrictionTypes.Base;

internal class LocalBaseRestriction : ILocalRestriction
{
    private readonly LocalBaseState<bool> _state = new(false);

    protected IRestriction? Restriction;

    public virtual void Destroy()
    {
        OnDestroy.SafeInvoke();
        _state.Destroy();
    }

    public virtual void Check()
    {
        _state.Check();
    }

    public event Action? OnChanged;
    public event Action? OnDestroy;

    public bool IsActive()
    {
        return _state.Value;
    }

    public virtual void Register(DynamicVariableSpace dynamicVariableSpace,
        IDynamicVariableSpaceSync dynamicVariableSpaceSync,
        IRestriction restriction)
    {
        Restriction = restriction;
        _state.Register(dynamicVariableSpace, dynamicVariableSpaceSync, restriction);
        _state.OnStateChanged += (_, _) => OnStateChanged();
    }

    protected virtual void OnStateChanged()
    {
        OnChanged.SafeInvoke();
    }
}