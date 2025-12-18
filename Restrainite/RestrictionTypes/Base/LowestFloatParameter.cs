using FrooxEngine;

namespace Restrainite.RestrictionTypes.Base;

internal sealed class LowestFloatParameter(
    float minValue = float.NegativeInfinity,
    float maxValue = float.PositiveInfinity)
    : IRestrictionParameter
{
    private SimpleState<float> LowestFloat { get; } = new(float.NaN);

    internal float Value => LowestFloat.Value;

    public bool Combine(IRestriction restriction, IEnumerable<IBaseState> states)
    {
        var lowestFloatValue = float.NaN;
        foreach (var baseState in states)
        {
            if (baseState is not LocalBaseState<float> localState) continue;
            var value = localState.Value;
            if (float.IsNaN(value)) continue;
            if (value < minValue) value = minValue;
            if (value > maxValue) value = maxValue;
            if (float.IsNaN(lowestFloatValue) || value < lowestFloatValue)
                lowestFloatValue = value;
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