using FrooxEngine;
using ResoniteModLoader;

namespace Restrainite.RestrictionTypes.Base;

internal class LocalBaseRestriction : ILocalRestriction
{
    private readonly IRestriction _restriction;
    private readonly LocalBaseState<bool> _state;

    internal LocalBaseRestriction(DynamicVariableSpace dynamicVariableSpace,
        IDynamicVariableSpace dynamicVariableSpaceSync,
        IRestriction restriction)
    {
        _restriction = restriction;
        _state = new LocalBaseState<bool>(false, dynamicVariableSpace, dynamicVariableSpaceSync,
            restriction);
        _state.OnStateChanged += Log;
        _state.OnStateChanged += (_, _, source) => OnStateChanged(source);
    }

    public virtual void Destroy()
    {
        _restriction.DestroyLocal(this);
        _state.Destroy();
    }

    public virtual void Check()
    {
        _state.Check();
    }

    private static void Log(IRestriction restriction, bool value, IDynamicVariableSpace source)
    {
        ResoniteMod.Msg($"Local state of {restriction.Name} changed to {value} by {source.AsString()}");
    }

    public bool IsActive()
    {
        return _state.Value;
    }

    protected virtual void OnStateChanged(IDynamicVariableSpace source)
    {
        _restriction.Update(source);
    }
}

internal class LocalBaseRestrictionBuilder : IBuilder<LocalBaseRestriction>
{
    public LocalBaseRestriction Build(DynamicVariableSpace dynamicVariableSpace,
        IDynamicVariableSpace dynamicVariableSpaceSync, IRestriction restriction)
    {
        return new LocalBaseRestriction(dynamicVariableSpace, dynamicVariableSpaceSync, restriction);
    }
}