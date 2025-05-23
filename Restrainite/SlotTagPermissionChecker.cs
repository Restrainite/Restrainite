using System;
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

        if (RestrainiteMod.IsRestricted(_deniedPrevention))
        {
            var denied = RestrainiteMod.GetStringSet(_deniedPrevention);
            if (denied.Contains(tag)) return PermissionType.ExplicitlyDenied;
        }

        if (RestrainiteMod.IsRestricted(_allowedPrevention))
        {
            var allowed = RestrainiteMod.GetStringSet(_allowedPrevention);
            return allowed.Contains(tag) ? PermissionType.ExplicitlyAllowed : PermissionType.Denied;
        }

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