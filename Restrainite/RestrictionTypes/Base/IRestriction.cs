using FrooxEngine;

namespace Restrainite.RestrictionTypes.Base;

public interface IRestriction
{
    int Index { get; set; }
    string Name { get; }
    string Description { get; }

    ILocalRestriction CreateLocal(DynamicVariableSpace dynamicVariableSpace,
        IDynamicVariableSpaceSync dynamicVariableSpaceSync);

    void CreateStatusComponent(Slot slot, string dynamicVariableSpaceName);

    void RegisterImpulseSender(ImpulseSender impulseSender);
}