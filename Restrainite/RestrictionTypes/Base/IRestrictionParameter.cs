using System.Collections.Generic;
using FrooxEngine;

namespace Restrainite.RestrictionTypes.Base;

internal interface IRestrictionParameter
{
    bool Combine(IRestriction restriction, IEnumerable<IBaseState> states);

    IBaseState CreateLocalState(DynamicVariableSpace dynamicVariableSpace,
        IDynamicVariableSpace dynamicVariableSpaceSync,
        IRestriction restriction);

    void CreateStatusComponent(IRestriction restriction, Slot slot, string dynamicVariableSpaceName);
}