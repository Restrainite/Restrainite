using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Restrainite.Enums;

namespace Restrainite.States;

internal class GlobalStateStringSet() : GlobalState<ImmutableStringSet>(ImmutableStringSet.Empty)
{
    protected override ImmutableStringSet Calculate(PreventionType preventionType,
        ICollection<IDynamicVariableSpaceWrapper> spaces)
    {
        return spaces.SelectMany(space => space.GetStringSetState(preventionType)).ToImmutableHashSet();
    }

    protected override bool IsCorrectType(PreventionType preventionType)
    {
        return preventionType.IsStringSetType();
    }
}