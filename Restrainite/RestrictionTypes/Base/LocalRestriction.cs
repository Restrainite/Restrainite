using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using FrooxEngine;

namespace Restrainite.RestrictionTypes.Base;

public class LocalRestriction
{
    private readonly ReadOnlyCollection<IBaseState> _localStates;
    private readonly IRestriction _restriction;
    private readonly LocalBaseState<bool> _state;

    internal LocalRestriction(DynamicVariableSpace dynamicVariableSpace,
        IDynamicVariableSpace dynamicVariableSpaceSync,
        IRestriction restriction, params IRestrictionParameter[] stateBuilder)
    {
        _restriction = restriction;
        _state = new LocalBaseState<bool>(false, dynamicVariableSpace, dynamicVariableSpaceSync,
            restriction, true);
        var localStates = new List<IBaseState> { _state };

        localStates.AddRange(stateBuilder.Select(builder =>
            builder.CreateLocalState(dynamicVariableSpace, dynamicVariableSpaceSync, restriction))
        );
        _localStates = localStates.AsReadOnly();
    }

    public void Check()
    {
        IDynamicVariableSpace? source = null;
        foreach (var localState in _localStates)
        {
            var changed = localState.Check(out var localSource);
            if (!changed) continue;
            source = localSource;
        }

        if (source != null) _restriction.Update(source);
    }

    public void Destroy()
    {
        foreach (var localState in _localStates) localState.Destroy();
        _restriction.DestroyLocal(this);
    }

    public IBaseState GetLocalState(int index)
    {
        return _localStates[index];
    }

    public bool IsActive()
    {
        return _state.Value;
    }
}