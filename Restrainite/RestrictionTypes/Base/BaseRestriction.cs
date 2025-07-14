using System;
using System.Collections.Generic;
using System.Linq;
using FrooxEngine;
using ResoniteModLoader;

namespace Restrainite.RestrictionTypes.Base;

internal abstract class BaseRestriction : IRestriction
{
    private readonly List<LocalRestriction> _localValues = [];

    private readonly SimpleState<bool> _state = new(false);
    private IRestrictionParameter[] _restrictionParameters = [];

    public bool IsRestricted => _state.Value;

    public int Index { get; private set; }

    public abstract string Name { get; }

    public abstract string Description { get; }

    public virtual bool IsDeprecated => false;

    public void Initialize(int index)
    {
        Index = index;
        if (_restrictionParameters.Length != 0) return;
        _restrictionParameters = InitRestrictionParameters();
    }

    public LocalRestriction CreateLocal(DynamicVariableSpace dynamicVariableSpace,
        IDynamicVariableSpace dynamicVariableSpaceSync)
    {
        var localValue =
            new LocalRestriction(dynamicVariableSpace, dynamicVariableSpaceSync, this, _restrictionParameters);
        lock (_localValues)
        {
            _localValues.Add(localValue);
        }

        return localValue;
    }

    public void CreateStatusComponent(Slot slot, string dynamicVariableSpaceName)
    {
        CreateStatusComponent(this, slot, dynamicVariableSpaceName, _state, a => a);
        CreateDescription(this, slot, dynamicVariableSpaceName);
        foreach (var restrictionParameter in _restrictionParameters)
            restrictionParameter.CreateStatusComponent(this, slot, dynamicVariableSpaceName);
    }

    public void RegisterImpulseSender(ImpulseSender impulseSender)
    {
        _state.OnStateChanged += impulseSender.SendDynamicImpulse;
        impulseSender.OnDestroy += sender => { _state.OnStateChanged -= sender.SendDynamicImpulse; };
    }

    public void Update(IDynamicVariableSpace source)
    {
        LocalRestriction[] localValues;
        lock (_localValues)
        {
            localValues = _localValues
                .Where(localValue => localValue.IsActive())
                .ToArray();
        }

        var changed = _state.SetIfChanged(this, localValues.Any());
        if (changed) Log(this, _state.Value, source);

        for (var index = 0; index < _restrictionParameters.Length; index++)
        {
            var restrictionParameter = _restrictionParameters[index];
            var localIndex = index + 1;
            var states = localValues.Select(restriction => restriction.GetLocalState(localIndex));
            changed = restrictionParameter.Combine(this, states) || changed;
        }

        if (!changed) return;
        OnChanged.SafeInvoke(this);
    }

    public void DestroyLocal(LocalRestriction localRestriction)
    {
        lock (_localValues)
        {
            _localValues.Remove(localRestriction);
        }

        Update(DestroyedDynamicVariableSpace.Instance);
    }

    protected virtual IRestrictionParameter[] InitRestrictionParameters()
    {
        return [];
    }

    private static void Log(IRestriction restriction, bool value, IDynamicVariableSpace source)
    {
        ResoniteMod.Msg($"Global state of {restriction.Name} changed to {value} by {source.AsString()}");
    }

    internal static void CreateStatusComponent<TS, TV>(
        IRestriction restriction,
        Slot slot,
        string dynamicVariableSpaceName,
        SimpleState<TS> state,
        Func<TS, TV> to)
    {
        var nameWithPrefix = dynamicVariableSpaceName + "/" + restriction.Name;
        var component = slot.GetComponentOrAttach<DynamicValueVariable<TV>>(out var attached,
            search => nameWithPrefix.Equals(search.VariableName.Value));

        component.VariableName.Value = nameWithPrefix;
        component.Value.Value = to(state.Value);
        component.Persistent = false;

        if (!attached) return;
        Action<IRestriction, TS> onUpdate = (_, value) =>
        {
            slot.RunSynchronously(() => component.Value.Value = to(value));
        };
        state.OnStateChanged += onUpdate;
        component.Disposing += _ => { state.OnStateChanged -= onUpdate; };
    }

    private static void CreateDescription(
        IRestriction restriction,
        Slot slot,
        string dynamicVariableSpaceName)
    {
        var nameWithPrefix = dynamicVariableSpaceName + "/" + restriction.Name + " Description";
        var component =
            slot.GetComponentOrAttach<DynamicValueVariable<string>>(search =>
                nameWithPrefix.Equals(search.VariableName.Value));

        component.VariableName.Value = nameWithPrefix;
        component.Value.Value = restriction.Description;
        component.Persistent = false;
    }

    internal static void CreateStatusRefComponent<TS, TV>(
        IRestriction restriction,
        Slot slot,
        string dynamicVariableSpaceName,
        SimpleState<TS> state,
        Func<TS, TV> to) where TV : class, IWorldElement
    {
        var nameWithPrefix = dynamicVariableSpaceName + "/" + restriction.Name;
        var component = slot.GetComponentOrAttach<DynamicReferenceVariable<TV>>(out var attached,
            search => nameWithPrefix.Equals(search.VariableName.Value));

        component.VariableName.Value = nameWithPrefix;
        component.Reference.Target = to(state.Value);
        component.Persistent = false;

        if (!attached) return;
        Action<IRestriction, TS> onUpdate = (_, value) =>
        {
            slot.RunSynchronously(() => component.Reference.Target = to(value));
        };
        state.OnStateChanged += onUpdate;
        component.Disposing += _ => { state.OnStateChanged -= onUpdate; };
    }

    public event Action<IRestriction>? OnChanged;
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