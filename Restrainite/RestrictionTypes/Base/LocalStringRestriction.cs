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
        StringState.OnStateChanged += OnStringStateChanged;
    }

    internal LocalBaseState<string> StringState { get; }

    public override void Destroy()
    {
        StringState.OnStateChanged -= OnStringStateChanged;
        base.Destroy();
        StringState.Destroy();
    }

    public override void Check()
    {
        base.Check();
        StringState.Check();
    }

    private static void OnStringStateChanged(IRestriction restriction, string value, IDynamicVariableSpace source)
    {
        restriction.Update(source);
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