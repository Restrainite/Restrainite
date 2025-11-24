using FrooxEngine;

namespace Restrainite.RestrictionTypes.Base;

internal sealed class LowestFloatParameter : IRestrictionParameter
{
    private SimpleState<float> LowestFloat { get; } = new(float.NaN);

    internal float Value => LowestFloat.Value;

    public bool Combine(IRestriction restriction, IEnumerable<IBaseState> states)
    {
        var lowestFloatValue = float.NaN;
        foreach (var baseState in states)
        {
            if (baseState is not LocalBaseState<float> localState) continue;
            if (!float.IsNaN(localState.Value) &&
                (float.IsNaN(lowestFloatValue) || localState.Value < lowestFloatValue))
                lowestFloatValue = localState.Value;
        }

        return LowestFloat.SetIfChanged(restriction, lowestFloatValue);
    }


    public IBaseState CreateLocalState(DynamicVariableSpace dynamicVariableSpace,
        IDynamicVariableSpace dynamicVariableSpaceSync,
        IRestriction restriction)
    {
        return new LocalBaseState<float>(float.NaN, dynamicVariableSpace, dynamicVariableSpaceSync,
            restriction, false);
    }

    public void CreateStatusComponent(IRestriction restriction, Slot slot, string dynamicVariableSpaceName)
    {
        BaseRestriction.CreateStatusComponent(restriction, slot, dynamicVariableSpaceName, LowestFloat, a => a);
    }
}