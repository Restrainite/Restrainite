using System.Collections.Generic;
using System.Linq;
using Restrainite.Enums;

namespace Restrainite.States;

internal class LocalStateStringSet : LocalState<string, ImmutableStringSet>
{
    private static readonly IEnumerable<PreventionType> ValidTypes =
        PreventionTypes.List.Where(type => type.IsStringSetType()).ToArray();

    internal LocalStateStringSet(IDynamicVariableSpaceWrapper dynamicVariableSpaceWrapper) :
        base(dynamicVariableSpaceWrapper, ImmutableStringSet.Empty, ValidTypes)
    {
        OnUpdateGlobal += RestrainiteMod.StringSetState.Update;
    }

    protected override ImmutableStringSet Transform(string? value)
    {
        return ImmutableStringSet.From(value);
    }
}