using FrooxEngine;

namespace Restrainite.RestrictionTypes.Base;

internal class LocalStringRestriction : LocalBaseRestriction
{
    internal LocalBaseState<string> StringState { get; } = new("");

    public override void Register(DynamicVariableSpace dynamicVariableSpace,
        IDynamicVariableSpaceSync dynamicVariableSpaceSync,
        IRestriction restriction)
    {
        base.Register(dynamicVariableSpace, dynamicVariableSpaceSync, restriction);
        StringState.Register(dynamicVariableSpace, dynamicVariableSpaceSync, restriction);
        StringState.OnStateChanged += (_, _) => base.OnStateChanged();
    }

    public override void Destroy()
    {
        base.Destroy();
        StringState.Destroy();
    }

    public override void Check()
    {
        base.Check();
        StringState.Check();
    }

    protected override void OnStateChanged()
    {
        StringState.Check(false);
        base.OnStateChanged();
    }
}