using System.Collections.Immutable;
using System.Linq;
using FrooxEngine;

namespace Restrainite.RestrictionTypes.Base;

internal abstract class StringSetRestriction : BaseRestriction<LocalStringSetRestriction>
{
    internal SimpleState<ImmutableStringSet> StringSet { get; } = new(ImmutableStringSet.Empty);

    public bool SetContains(string userId)
    {
        return StringSet.Value.Contains(userId);
    }

    protected override bool Combine(LocalStringSetRestriction[] restrictions)
    {
        var baseChanged = base.Combine(restrictions);
        var state = restrictions
            .SelectMany(space => space.StringSet.Value)
            .ToImmutableHashSet();
        var changed = StringSet.SetIfChanged(this, state);
        if (changed) LogChange("Global string", StringSet.Value.ToString());
        return changed || baseChanged;
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