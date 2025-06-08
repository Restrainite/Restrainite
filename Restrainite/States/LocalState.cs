using System;
using System.Collections.Generic;
using Restrainite.Enums;

namespace Restrainite.States;

internal abstract class LocalState<TGame, TInternal> : State<TInternal> where TInternal : notnull
{
    private readonly IDynamicVariableSpaceWrapper _dynamicVariableSpaceWrapper;

    private readonly DynamicVariableKeyChangeListener<PreventionType, TGame>[]
        _listeners =
            PreventionTypes.CreateDefaultArray<DynamicVariableKeyChangeListener<PreventionType, TGame>>(null!);

    private readonly string _typeName = "Local " + typeof(TGame).Name;

    private readonly IEnumerable<PreventionType> _validTypes;

    internal LocalState(IDynamicVariableSpaceWrapper dynamicVariableSpaceWrapper, TInternal defaultValue,
        IEnumerable<PreventionType> validTypes) : base(defaultValue)
    {
        _dynamicVariableSpaceWrapper = dynamicVariableSpaceWrapper;
        _validTypes = validTypes;
        if (!dynamicVariableSpaceWrapper.GetDynamicVariableSpace(out var dynamicVariableSpace)) return;
        foreach (var preventionType in PreventionTypes.List)
            _listeners[(int)preventionType] = new DynamicVariableKeyChangeListener<PreventionType, TGame>(
                dynamicVariableSpace,
                preventionType,
                preventionType.ToExpandedString());
    }

    internal event Action<PreventionType, IDynamicVariableSpaceWrapper, ICollection<IDynamicVariableSpaceWrapper>>?
        OnUpdateGlobal;

    internal void Register()
    {
        foreach (var preventionType in _validTypes)
        {
            _listeners[(int)preventionType].Register(Update);
            UpdateGlobal(preventionType);
        }
    }

    internal void Unregister()
    {
        foreach (var preventionType in _validTypes)
        {
            _listeners[(int)preventionType].Unregister(Update);
            UpdateGlobal(preventionType);
        }
    }

    private void Update(PreventionType preventionType, TGame value)
    {
        var newValue = Transform(value);
        if (!_dynamicVariableSpaceWrapper.IsActiveForLocalUser(preventionType)) newValue = DefaultValue;
        if (!SetIfChanged(preventionType, newValue)) return;
        LogChange(_typeName, preventionType, newValue, _dynamicVariableSpaceWrapper);
        UpdateGlobal(preventionType);
    }

    private void UpdateGlobal(PreventionType preventionType)
    {
        OnUpdateGlobal.SafeInvoke(preventionType, _dynamicVariableSpaceWrapper,
            _dynamicVariableSpaceWrapper.GetActiveSpaces(preventionType));
    }

    internal void CheckState()
    {
        foreach (var preventionType in _validTypes)
            Update(preventionType, _listeners[(int)preventionType].DynamicValue);
    }

    protected abstract TInternal Transform(TGame value);
}