using FrooxEngine;

namespace Restrainite.RestrictionTypes.Base;

internal class LocalStringSetRestriction : LocalBaseRestriction
{
    private DynamicVariableChangeListener<string>? _changeListener;
    internal SimpleState<ImmutableStringSet> StringSet { get; } = new(ImmutableStringSet.Empty);

    public override void Register(DynamicVariableSpace dynamicVariableSpace, IRestriction.IsValid isValid,
        IRestriction restriction)
    {
        base.Register(dynamicVariableSpace, isValid, restriction);
        _changeListener = new DynamicVariableChangeListener<string>(dynamicVariableSpace, restriction.Name);
        _changeListener.Register(OnValueChanged);
        CheckValue();
    }

    private void CheckValue()
    {
        OnValueChanged(_changeListener?.DynamicValue ?? "");
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
        CheckValue();
    }

    private void OnValueChanged(string value)
    {
        OnValueChanged(value, true);
    }

    private void OnValueChanged(string value, bool trigger)
    {
        if (Restriction == null) return;
        var changed = StringSet.SetIfChanged(Restriction,
            IsActive() ? ImmutableStringSet.From(value) : ImmutableStringSet.Empty);
        if (!changed) return;
        _changeListener?.LogChange("Local string", Restriction, StringSet.Value.ToString());
        if (trigger) OnStateChanged();
    }

    protected override void OnStateChanged()
    {
        OnValueChanged(_changeListener?.DynamicValue ?? "", false);
        base.OnStateChanged();
    }
}