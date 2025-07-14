namespace Restrainite.RestrictionTypes.Base;

public interface IBaseState
{
    bool Check(out IDynamicVariableSpace source);

    void Destroy();
}