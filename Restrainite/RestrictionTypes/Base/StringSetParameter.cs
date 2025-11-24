using System.Collections.Immutable;
using FrooxEngine;

namespace Restrainite.RestrictionTypes.Base;

internal sealed class StringSetParameter : IRestrictionParameter
{
    private SimpleState<ImmutableStringSet> StringSet { get; } = new(ImmutableStringSet.Empty);

    public ImmutableStringSet Value => StringSet.Value;

    public bool Combine(IRestriction restriction, IEnumerable<IBaseState> states)
    {
        var builder = ImmutableHashSet.CreateBuilder<string>();
        foreach (var baseState in states)
        {
            if (baseState is not LocalBaseState<string> localState) continue;
            builder.UnionWith(SplitValues(localState.Value));
        }

        var state = builder.ToImmutable();
        var changed = StringSet.SetIfChanged(restriction, state);
        return changed;
    }

    public IBaseState CreateLocalState(DynamicVariableSpace dynamicVariableSpace,
        IDynamicVariableSpace dynamicVariableSpaceSync,
        IRestriction restriction)
    {
        return new LocalBaseState<string>("", dynamicVariableSpace, dynamicVariableSpaceSync, restriction, false);
    }

    public void CreateStatusComponent(IRestriction restriction, Slot slot, string dynamicVariableSpaceName)
    {
        BaseRestriction.CreateStatusComponent(restriction, slot, dynamicVariableSpaceName, StringSet,
            a => a.ToString());
    }

    public bool Contains(string value)
    {
        return !string.IsNullOrEmpty(value) &&
               StringSet.Value.Contains(value);
    }

    private static IEnumerable<string> SplitValues(string? commaSeparatedList)
    {
        var splitArray = commaSeparatedList?.Split([','], StringSplitOptions.RemoveEmptyEntries) ?? [];
        return splitArray.Select(t => t.Trim())
            .Where(trimmed => trimmed.Length != 0);
    }
}