using FrooxEngine;

namespace Restrainite.RestrictionTypes.Base;

internal class LocalLowestFloatRestriction : LocalBaseRestriction
{
    private DynamicVariableChangeListener<float>? _changeListener;
    internal SimpleState<float> LowestFloat { get; } = new(float.NaN);

    public override void Register(DynamicVariableSpace dynamicVariableSpace, IRestriction.IsValid isValid,
        IRestriction restriction)
    {
        base.Register(dynamicVariableSpace, isValid, restriction);
        _changeListener = new DynamicVariableChangeListener<float>(dynamicVariableSpace, restriction.Name);
        _changeListener.Register(OnValueChanged);
        OnValueChanged(_changeListener?.DynamicValue ?? float.NaN);
    }

    public override void Destroy()
    {
        base.Destroy();
        if (_changeListener == null) return;
        _changeListener.Unregister(OnValueChanged);
        _changeListener = null;
    }

    public override void Check()
    {
        base.Check();
        OnValueChanged(_changeListener?.DynamicValue ?? float.NaN);
    }

    private void OnValueChanged(float value)
    {
        OnValueChanged(value, true);
    }

    private void OnValueChanged(float value, bool trigger)
    {
        if (Restriction == null) return;
        var changed = LowestFloat.SetIfChanged(Restriction, IsActive() ? value : float.NaN);
        if (!changed) return;
        _changeListener?.LogChange("Local float", Restriction, LowestFloat.Value);
        if (trigger) OnStateChanged();
    }

    protected override void OnStateChanged()
    {
        OnValueChanged(_changeListener?.DynamicValue ?? float.NaN, false);
        base.OnStateChanged();
    }
}