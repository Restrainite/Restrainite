using System.Collections.Generic;
using FrooxEngine;
using Restrainite.Enums;

namespace Restrainite.States;

internal interface IDynamicVariableSpaceWrapper
{
    float GetLowestFloatState(PreventionType preventionType);

    ImmutableStringSet GetStringSetState(PreventionType preventionType);

    bool GetDynamicVariableSpace(out DynamicVariableSpace o);

    bool IsActiveForLocalUser(PreventionType preventionType);

    public ICollection<IDynamicVariableSpaceWrapper> GetActiveSpaces(PreventionType preventionType);
}