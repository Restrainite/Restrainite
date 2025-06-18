using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using FrooxEngine;

namespace Restrainite.RestrictionTypes.Base;

internal abstract class StringSetRestriction : BaseRestriction<LocalStringRestriction>
{
    internal SimpleState<ImmutableStringSet> StringSet { get; } = new(ImmutableStringSet.Empty);

    public bool SetContains(string value)
    {
        return !string.IsNullOrEmpty(value) &&
               StringSet.Value.Contains(value);
    }

    protected override bool Combine(LocalStringRestriction[] restrictions)
    {
        var baseChanged = base.Combine(restrictions);
        if (restrictions.Length == 0)
            return baseChanged;

        var builder = ImmutableHashSet.CreateBuilder<string>();
        foreach (var restriction in restrictions)
            builder.UnionWith(SplitValues(restriction.StringState.Value));
        var state = builder.ToImmutable();
        var changed = StringSet.SetIfChanged(this, state);
        if (changed) LogChange("Global string", StringSet.Value.ToString());
        return changed || baseChanged;
    }

    private static IEnumerable<string> SplitValues(string? commaSeparatedList)
    {
        var splitArray = commaSeparatedList?.Split(',') ?? [];
        return splitArray.Select(t => t.Trim())
            .Where(trimmed => trimmed.Length != 0);
    }

    public override void CreateStatusComponent(Slot slot, string dynamicVariableSpaceName)
    {
        base.CreateStatusComponent(slot, dynamicVariableSpaceName);
        CreateStatusComponent(slot,
            dynamicVariableSpaceName,
            StringSet,
            a => a.ToString());
    }

    public override void RegisterImpulseSender(ImpulseSender impulseSender)
    {
        base.RegisterImpulseSender(impulseSender);
        RegisterStateImpulseSender(impulseSender, StringSet, a => a.ToString());
    }
}