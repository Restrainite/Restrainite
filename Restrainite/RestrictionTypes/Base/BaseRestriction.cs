using System;
using System.Collections.Generic;
using System.Linq;
using FrooxEngine;
using ResoniteModLoader;

namespace Restrainite.RestrictionTypes.Base;

internal abstract class BaseRestriction : BaseRestriction<LocalBaseRestriction>
{
}

internal abstract class BaseRestriction<T> : IRestriction where T : LocalBaseRestriction, new()
{
    private readonly List<T> _localValues = [];

    internal SimpleState<bool> State { get; } = new(false);

    public bool IsRestricted => State.Value;

    public int Index { get; set; }

    public abstract string Name { get; }

    public abstract string Description { get; }

    public ILocalRestriction CreateLocal(DynamicVariableSpace dynamicVariableSpace,
        IDynamicVariableSpaceSync dynamicVariableSpaceSync)
    {
        var localValue = new T();
        localValue.Register(dynamicVariableSpace, dynamicVariableSpaceSync, this);
        localValue.OnChanged += Update;
        localValue.OnDestroy += () =>
        {
            localValue.OnChanged -= Update;
            lock (_localValues)
            {
                _localValues.Remove(localValue);
            }
        };
        lock (_localValues)
        {
            _localValues.Add(localValue);
        }

        return localValue;
    }

    public virtual void CreateStatusComponent(Slot slot, string dynamicVariableSpaceName)
    {
        CreateStatusComponent(slot, dynamicVariableSpaceName, State, a => a);
    }

    public virtual void RegisterImpulseSender(ImpulseSender impulseSender)
    {
        RegisterStateImpulseSender(impulseSender, State, a => a);
    }

    protected void RegisterStateImpulseSender<TS, TV>(ImpulseSender impulseSender, SimpleState<TS> state,
        Func<TS, TV> to) where TS : IEquatable<TS>
    {
        Action<IRestriction, TS> action = (restriction, value) =>
        {
            impulseSender.SendDynamicImpulse(restriction, to(value));
        };
        state.OnStateChanged += action;
        impulseSender.OnDestroy += () => { state.OnStateChanged -= action; };
    }

    protected void CreateStatusComponent<TS, TV>(
        Slot slot,
        string dynamicVariableSpaceName,
        SimpleState<TS> state,
        Func<TS, TV> to) where TS : IEquatable<TS>
    {
        var nameWithPrefix = dynamicVariableSpaceName + "/" + Name;
        var component = slot.GetComponentOrAttach<DynamicValueVariable<TV>>(out var attached,
            search => nameWithPrefix.Equals(search.VariableName.Value));

        component.VariableName.Value = nameWithPrefix;
        component.Value.Value = to(state.Value);
        component.Persistent = false;

        if (!attached) return;
        Action<IRestriction, TS> onUpdate = (_, value) =>
        {
            slot.RunInUpdates(0, () => component.Value.Value = to(value));
        };
        state.OnStateChanged += onUpdate;
        component.Disposing += _ => { state.OnStateChanged -= onUpdate; };
    }

    public event Action<IRestriction>? OnChanged;

    protected virtual bool Combine(T[] values)
    {
        var changed = State.SetIfChanged(this, values.Any());
        if (!changed) return false;
        LogChange("Global bool", State.Value);
        return changed;
    }

    private void Update()
    {
        T[] localValues;
        lock (_localValues)
        {
            localValues = _localValues
                .Where(localValue => localValue.IsActive())
                .ToArray();
        }

        var changed = Combine(localValues);
        if (!changed) return;

        OnChanged.SafeInvoke(this);
    }

    protected void LogChange<TV>(string typeName, TV value)
    {
        ResoniteMod.Msg($"{typeName} of {Name} changed to '{value}'.");
    }
}