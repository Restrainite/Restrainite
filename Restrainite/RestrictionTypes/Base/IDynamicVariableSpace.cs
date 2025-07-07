namespace Restrainite.RestrictionTypes.Base;

public interface IDynamicVariableSpace
{
    bool IsActiveForLocalUser(IRestriction restriction);

    string AsString();
}