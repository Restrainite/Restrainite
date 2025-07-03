using FrooxEngine;

namespace Restrainite.RestrictionTypes.Base;

internal interface IBuilder<out T> where T : LocalBaseRestriction
{
    T Build(DynamicVariableSpace dynamicVariableSpace,
        IDynamicVariableSpace dynamicVariableSpaceSync,
        IRestriction restriction);
}