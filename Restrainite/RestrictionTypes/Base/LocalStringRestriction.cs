using FrooxEngine;

namespace Restrainite.RestrictionTypes.Base;

internal class LocalStringRestriction : LocalBaseRestriction
{
    internal LocalStringRestriction(DynamicVariableSpace dynamicVariableSpace,
        IDynamicVariableSpace dynamicVariableSpaceSync,
        IRestriction restriction) : base(dynamicVariableSpace, dynamicVariableSpaceSync, restriction)
    {
        StringState = new LocalBaseState<string>(
            "", dynamicVariableSpace, dynamicVariableSpaceSync, restriction);
        StringState.OnStateChanged += (_, _, source) => restriction.Update(source);
    }

    internal LocalBaseState<string> StringState { get; }

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

    protected override void OnStateChanged(IDynamicVariableSpace source)
    {
        StringState.Check(false);
        base.OnStateChanged(source);
    }
}

internal class LocalStringRestrictionBuilder : IBuilder<LocalStringRestriction>
{
    public LocalStringRestriction Build(DynamicVariableSpace dynamicVariableSpace,
        IDynamicVariableSpace dynamicVariableSpaceSync, IRestriction restriction)
    {
        return new LocalStringRestriction(dynamicVariableSpace, dynamicVariableSpaceSync, restriction);
    }
}