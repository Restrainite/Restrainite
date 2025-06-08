using System;
using System.Collections.Generic;
using Restrainite.Enums;

namespace Restrainite.States;

internal abstract class GlobalState<T>(T defaultValue) : State<T>(defaultValue) where T : notnull
{
    private readonly string _typeName = "Global " + typeof(T).Name;

    /**
     * OnChanged will fire, when the value is changed. It will take into account, if
     * the restriction is disabled by the user. It will run in the update cycle of the world that triggered the
     * change. This doesn't have to be the focused world, so make sure, that any write operation are run in the next
     * update cycle. The value is debounced, meaning it will only trigger, if it actually changes.
     */
    internal event Action<PreventionType, T>? OnChanged;

    internal void Update(PreventionType preventionType, IDynamicVariableSpaceWrapper source,
        ICollection<IDynamicVariableSpaceWrapper> activeSpaces)
    {
        if (!IsCorrectType(preventionType)) return;
        var globalState = Calculate(preventionType, activeSpaces);
        if (!SetIfChanged(preventionType, globalState)) return;
        LogChange(_typeName, preventionType, globalState, source);
        NotifyChange(preventionType, globalState, source);
    }

    protected abstract bool IsCorrectType(PreventionType preventionType);

    protected abstract T Calculate(PreventionType preventionType, ICollection<IDynamicVariableSpaceWrapper> spaces);

    private void NotifyChange(PreventionType preventionType, T value, IDynamicVariableSpaceWrapper source)
    {
        if (!source.GetDynamicVariableSpace(out var dynamicVariableSpace)) return;
        dynamicVariableSpace.World.RunInUpdates(0, () => OnChanged.SafeInvoke(preventionType, value));
    }
}