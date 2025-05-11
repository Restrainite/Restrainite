using FrooxEngine;
using Restrainite.Enums;

namespace Restrainite;

public class SlotTagPermissionChecker
{
    private readonly PreventionType _allowedPrevention;
    private readonly PreventionType _deniedPrevention;

    internal SlotTagPermissionChecker(PreventionType allowedPrevention, PreventionType deniedPrevention)
    {
        _allowedPrevention = allowedPrevention;
        _deniedPrevention = deniedPrevention;
    }

    internal bool IsAllowed(Slot? slot)
    {
        var slotPermission = CheckPermissionForSlot(slot);

        if (slotPermission == PermissionType.ExplicitlyDenied || slot?.GetObjectRoot() is not Slot objectRootSlot)
            return slotPermission.ToBool();

        var objectRootSlotPermission = CheckPermissionForSlot(objectRootSlot);

        return objectRootSlotPermission switch
        {
            PermissionType.ExplicitlyAllowed => true,
            PermissionType.ExplicitlyDenied => false,
            _ => slotPermission.ToBool()
        };
    }

    private PermissionType CheckPermissionForSlot(Slot? slot)
    {
        var tag = slot == null || string.IsNullOrEmpty(slot.Tag) ? "null" : slot.Tag;

        if (RestrainiteMod.IsRestricted(_deniedPrevention))
        {
            var denied = RestrainiteMod.GetStrings(_deniedPrevention);
            if (denied.Contains(tag)) return PermissionType.ExplicitlyDenied;
        }

        if (RestrainiteMod.IsRestricted(_allowedPrevention))
        {
            var allowed = RestrainiteMod.GetStrings(_allowedPrevention);
            return allowed.Contains(tag) ? PermissionType.ExplicitlyAllowed : PermissionType.Denied;
        }

        return PermissionType.Allowed;
    }
}