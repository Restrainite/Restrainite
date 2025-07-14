using FrooxEngine;

namespace Restrainite.RestrictionTypes.Base;

public interface IRestriction
{
    int Index { get; }
    string Name { get; }
    string Description { get; }

    bool IsDeprecated { get; }

    LocalRestriction CreateLocal(DynamicVariableSpace dynamicVariableSpace,
        IDynamicVariableSpace dynamicVariableSpaceSync);

    void CreateStatusComponent(Slot slot, string dynamicVariableSpaceName);

    void RegisterImpulseSender(ImpulseSender impulseSender);

    void Update(IDynamicVariableSpace source);

    void DestroyLocal(LocalRestriction localBaseRestriction);

    void Initialize(int index);
}