using System;

namespace Restrainite.Enums;

public enum PermissionType
{
    Allowed,
    Denied,
    ExplicitlyAllowed,
    ExplicitlyDenied
}

internal static class PermissionTypes
{
    internal static bool ToBool(this PermissionType type)
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
}