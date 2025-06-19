using FrooxEngine;

namespace Restrainite.RestrictionTypes.Base;

internal abstract class LowestFloatRestriction : BaseRestriction<LocalFloatRestriction>
{
    internal SimpleState<float> LowestFloat { get; } = new(float.NaN);

    protected override bool Combine(LocalFloatRestriction[] restrictions)
    {
        var baseChanged = base.Combine(restrictions);

        var lowestFloatValue = float.NaN;
        foreach (var restriction in restrictions)
            if (!float.IsNaN(restriction.FloatState.Value) &&
                (float.IsNaN(lowestFloatValue) || restriction.FloatState.Value < lowestFloatValue))
                lowestFloatValue = restriction.FloatState.Value;

        var changed = LowestFloat.SetIfChanged(this, lowestFloatValue);
        if (changed) LogChange("Global float", LowestFloat.Value);

        return changed || baseChanged;
    }

    public override void CreateStatusComponent(Slot slot, string dynamicVariableSpaceName)
    {
        base.CreateStatusComponent(slot, dynamicVariableSpaceName);
        CreateStatusComponent(slot, dynamicVariableSpaceName, LowestFloat, a => a);
    }

    public override void RegisterImpulseSender(ImpulseSender impulseSender)
    {
        base.RegisterImpulseSender(impulseSender);
        RegisterStateImpulseSender(impulseSender, LowestFloat, a => a);
    }
}