namespace Restrainite.RestrictionTypes.Base;

public interface IDynamicVariableSpaceSync
{
    bool IsActiveForLocalUser(IRestriction restriction);
}