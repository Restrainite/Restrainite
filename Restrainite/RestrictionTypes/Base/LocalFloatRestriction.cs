using FrooxEngine;

namespace Restrainite.RestrictionTypes.Base;

internal class LocalFloatRestriction : LocalBaseRestriction
{
    internal LocalBaseState<float> LowestFloat { get; } = new(float.NaN);

    public override void Register(DynamicVariableSpace dynamicVariableSpace,
        IDynamicVariableSpaceSync dynamicVariableSpaceSync,
        IRestriction restriction)
    {
        base.Register(dynamicVariableSpace, dynamicVariableSpaceSync, restriction);
        LowestFloat.Register(dynamicVariableSpace, dynamicVariableSpaceSync, restriction);
        LowestFloat.OnStateChanged += (_, _) => base.OnStateChanged();
    }

    public override void Destroy()
    {
        base.Destroy();
        LowestFloat.Destroy();
    }

    public override void Check()
    {
        base.Check();
        LowestFloat.Check();
    }

    protected override void OnStateChanged()
    {
        LowestFloat.Check(false);
        base.OnStateChanged();
    }
}