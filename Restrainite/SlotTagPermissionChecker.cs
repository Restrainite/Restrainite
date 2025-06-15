using System;
using FrooxEngine;
using Restrainite.RestrictionTypes.Base;

namespace Restrainite;

public class SlotTagPermissionChecker
{
    private readonly StringSetRestriction _allowedPrevention;
    private readonly StringSetRestriction _deniedPrevention;

    internal SlotTagPermissionChecker(StringSetRestriction allowedPrevention,
        StringSetRestriction deniedPrevention)
    {
        _allowedPrevention = allowedPrevention;
        _deniedPrevention = deniedPrevention;
    }

    internal bool IsAllowed(Slot? slot)
    {
        var slotPermission = CheckPermissionForSlot(slot);

        if (slotPermission == PermissionType.ExplicitlyDenied || slot?.GetObjectRoot() is not Slot objectRootSlot)
            return PermissionToBool(slotPermission);

        var objectRootSlotPermission = CheckPermissionForSlot(objectRootSlot);

        return objectRootSlotPermission switch
        {
            PermissionType.ExplicitlyAllowed => true,
            PermissionType.ExplicitlyDenied => false,
            _ => PermissionToBool(slotPermission)
        };
    }

    private PermissionType CheckPermissionForSlot(Slot? slot)
    {
        var tag = slot == null || string.IsNullOrEmpty(slot.Tag) ? "null" : slot.Tag;

        if (_deniedPrevention.IsRestricted)
            if (_deniedPrevention.SetContains(tag))
                return PermissionType.ExplicitlyDenied;

        if (_allowedPrevention.IsRestricted)
            return _allowedPrevention.SetContains(tag) ? PermissionType.ExplicitlyAllowed : PermissionType.Denied;

        return PermissionType.Allowed;
    }

    private static bool PermissionToBool(PermissionType type)
    {
        return type switch
        {
            PermissionType.Allowed => true,
            PermissionType.Denied => false,
            PermissionType.ExplicitlyAllowed => true,
            PermissionType.ExplicitlyDenied => false,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private enum PermissionType
    {
        Allowed,
        Denied,
        ExplicitlyAllowed,
        ExplicitlyDenied
    }
}