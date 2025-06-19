using FrooxEngine;

namespace Restrainite.RestrictionTypes.Base;

internal class LocalFloatRestriction : LocalBaseRestriction
{
    internal LocalBaseState<float> FloatState { get; } = new(float.NaN);

    public override void Register(DynamicVariableSpace dynamicVariableSpace,
        IDynamicVariableSpaceSync dynamicVariableSpaceSync,
        IRestriction restriction)
    {
        base.Register(dynamicVariableSpace, dynamicVariableSpaceSync, restriction);
        FloatState.Register(dynamicVariableSpace, dynamicVariableSpaceSync, restriction);
        FloatState.OnStateChanged += (_, _) => base.OnStateChanged();
    }

    public override void Destroy()
    {
        base.Destroy();
        FloatState.Destroy();
    }

    public override void Check()
    {
        base.Check();
        FloatState.Check();
    }

    protected override void OnStateChanged()
    {
        FloatState.Check(false);
        base.OnStateChanged();
    }
}