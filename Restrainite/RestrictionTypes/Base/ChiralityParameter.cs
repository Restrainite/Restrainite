using System.Collections.Generic;
using FrooxEngine;

namespace Restrainite.RestrictionTypes.Base;

public class ChiralityParameter : IRestrictionParameter
{
    private SimpleState<Chirality?> Chirality { get; } = new(null);

    public Chirality? Value => Chirality.Value;

    public bool Combine(IRestriction restriction, IEnumerable<IBaseState> states)
    {
        Chirality? chiralityValue = null;
        foreach (var baseState in states)
        {
            if (baseState is not LocalBaseState<Chirality?> localState) continue;
            if (localState.Value == null)
            {
                chiralityValue = null;
                break;
            }

            if (chiralityValue == null)
            {
                chiralityValue = localState.Value;
            }
            else if (chiralityValue != localState.Value)
            {
                chiralityValue = null;
                break;
            }
        }

        return Chirality.SetIfChanged(restriction, chiralityValue);
    }

    public IBaseState CreateLocalState(DynamicVariableSpace dynamicVariableSpace,
        IDynamicVariableSpace dynamicVariableSpaceSync,
        IRestriction restriction)
    {
        return new LocalBaseState<Chirality?>(null, dynamicVariableSpace, dynamicVariableSpaceSync,
            restriction, false);
    }

    public void CreateStatusComponent(IRestriction restriction, Slot slot, string dynamicVariableSpaceName)
    {
        BaseRestriction.CreateStatusComponent(restriction, slot, dynamicVariableSpaceName, Chirality, a => a);
    }

    public bool IsRestricted(Chirality? chirality)
    {
        return Chirality.Value == null || Chirality.Value == chirality;
    }
}