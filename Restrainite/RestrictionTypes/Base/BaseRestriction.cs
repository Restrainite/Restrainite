using System;
using System.Collections.Generic;
using System.Linq;
using FrooxEngine;
using ResoniteModLoader;

namespace Restrainite.RestrictionTypes.Base;

internal abstract class BaseRestriction : BaseRestriction<LocalBaseRestriction, LocalBaseRestrictionBuilder>
{
}

internal abstract class BaseRestriction<T, TB> : IRestriction
    where T : LocalBaseRestriction, ILocalRestriction where TB : IBuilder<T>, new()
{
    private readonly List<T> _localValues = [];

    private readonly SimpleState<bool> _state = new(false);

    public bool IsRestricted => _state.Value;

    public int Index { get; set; }

    public abstract string Name { get; }

    public abstract string Description { get; }

    public ILocalRestriction CreateLocal(DynamicVariableSpace dynamicVariableSpace,
        IDynamicVariableSpace dynamicVariableSpaceSync)
    {
        var localValue = new TB().Build(dynamicVariableSpace, dynamicVariableSpaceSync, this);
        lock (_localValues)
        {
            _localValues.Add(localValue);
        }

        return localValue;
    }

    public void DestroyLocal(ILocalRestriction localRestriction)
    {
        if (localRestriction is T localBaseRestriction)
            lock (_localValues)
            {
                _localValues.Remove(localBaseRestriction);
            }

        Update(DestroyedDynamicVariableSpace.Instance);
    }

    public virtual void CreateStatusComponent(Slot slot, string dynamicVariableSpaceName)
    {
        CreateStatusComponent(slot, dynamicVariableSpaceName, _state, a => a);
    }

    public void RegisterImpulseSender(ImpulseSender impulseSender)
    {
        _state.OnStateChanged += impulseSender.SendDynamicImpulse;
        impulseSender.OnDestroy += sender => { _state.OnStateChanged -= sender.SendDynamicImpulse; };
    }

    public void Update(IDynamicVariableSpace source)
    {
        T[] localValues;
        lock (_localValues)
        {
            localValues = _localValues
                .Where(localValue => localValue.IsActive())
                .ToArray();
        }

        var changed = Combine(localValues, source);
        if (!changed) return;

        OnChanged.SafeInvoke(this);
    }

    private static void Log(IRestriction restriction, bool value, IDynamicVariableSpace source)
    {
        ResoniteMod.Msg($"Global state of {restriction.Name} changed to {value} by {source.AsString()}");
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
        Action<IRestriction, TS, IDynamicVariableSpace> onUpdate = (_, value, _) =>
        {
            slot.RunSynchronously(() => component.Value.Value = to(value));
        };
        state.OnStateChanged += onUpdate;
        component.Disposing += _ => { state.OnStateChanged -= onUpdate; };
    }

    public event Action<IRestriction>? OnChanged;

    protected virtual bool Combine(T[] values, IDynamicVariableSpace source)
    {
        var changed = _state.SetIfChanged(this, values.Any(), source);
        if (!changed) return false;
        Log(this, _state.Value, source);
        return changed;
    }
}

internal class DestroyedDynamicVariableSpace : IDynamicVariableSpace
{
    private const string DestroyedDynamicVariableSpaceString = "Destroyed DynamicVariableSpace";
    internal static readonly DestroyedDynamicVariableSpace Instance = new();

    public bool IsActiveForLocalUser(IRestriction restriction)
    {
        return false;
    }

    public string AsString()
    {
        return DestroyedDynamicVariableSpaceString;
    }
}