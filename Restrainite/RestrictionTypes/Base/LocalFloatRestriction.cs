using FrooxEngine;

namespace Restrainite.RestrictionTypes.Base;

internal class LocalFloatRestriction : LocalBaseRestriction
{
    internal LocalFloatRestriction(DynamicVariableSpace dynamicVariableSpace,
        IDynamicVariableSpace dynamicVariableSpaceSync,
        IRestriction restriction) : base(dynamicVariableSpace, dynamicVariableSpaceSync, restriction)
    {
        FloatState = new LocalBaseState<float>(float.NaN,
            dynamicVariableSpace,
            dynamicVariableSpaceSync,
            restriction);
        FloatState.OnStateChanged += OnFloatStateChanged;
    }

    internal LocalBaseState<float> FloatState { get; }

    public override void Destroy()
    {
        FloatState.OnStateChanged -= OnFloatStateChanged;
        base.Destroy();
        FloatState.Destroy();
    }

    public override void Check()
    {
        base.Check();
        FloatState.Check();
    }

    private static void OnFloatStateChanged(IRestriction restriction, float value, IDynamicVariableSpace source)
    {
        restriction.Update(source);
    }

    protected override void OnStateChanged(IDynamicVariableSpace source)
    {
        FloatState.Check(false);
        base.OnStateChanged(source);
    }
}

internal class LocalFloatRestrictionBuilder : IBuilder<LocalFloatRestriction>
{
    public LocalFloatRestriction Build(DynamicVariableSpace dynamicVariableSpace,
        IDynamicVariableSpace dynamicVariableSpaceSync, IRestriction restriction)
    {
        return new LocalFloatRestriction(dynamicVariableSpace, dynamicVariableSpaceSync, restriction);
    }
}