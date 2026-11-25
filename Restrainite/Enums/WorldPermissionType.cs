using System.Text.RegularExpressions;
using FrooxEngine;
using SkyFrost.Base;

namespace Restrainite.Enums;

internal enum WorldPermissionType
{
    AnyoneHidden,
    AnyoneNonHidden,
    RegisteredUsersHidden,
    RegisteredUsersNonHidden,
    ContactsPlusHidden,
    ContactsPlusNonHidden,
    ContactsHidden,
    ContactsNonHidden,
    LanHidden,
    LanNonHidden,
    PrivateHidden,
    PrivateNonHidden
}

internal static partial class WorldPermissionTypes
{
    internal static readonly IEnumerable<WorldPermissionType> List =
        Enum.GetValues<WorldPermissionType>();

    private static readonly Dictionary<WorldPermissionType, string> Dictionary =
        List.ToDictionary(l => l,
            l => WordBoundaryRegex.Replace(l.ToString(), "$1 $2"));

    [GeneratedRegex("([a-z])([A-Z])")]
    private static partial Regex WordBoundaryRegex { get; }

    internal static string AsExpandedString(this WorldPermissionType type)
    {
        return Dictionary[type];
    }

    internal static PresetChangeType Default(this WorldPermissionType type)
    {
        return type switch
        {
            WorldPermissionType.AnyoneHidden or WorldPermissionType.AnyoneNonHidden
                or WorldPermissionType.RegisteredUsersHidden
                or WorldPermissionType.RegisteredUsersNonHidden or WorldPermissionType.ContactsPlusHidden
                or WorldPermissionType.ContactsPlusNonHidden or WorldPermissionType.ContactsNonHidden
                or WorldPermissionType.LanNonHidden => PresetChangeType.None,
            WorldPermissionType.LanHidden or WorldPermissionType.ContactsHidden or WorldPermissionType.PrivateHidden
                or WorldPermissionType.PrivateNonHidden => PresetChangeType.DoNotChange,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    internal static WorldPermissionType ToWorldPermissionType(this World world)
    {
        var sessionAccessLevel = world.Configuration.AccessLevel.Value;
        var hidden = world.HideFromListing;
        return ToWorldPermissionType(sessionAccessLevel, hidden);
    }

    private static WorldPermissionType ToWorldPermissionType(SessionAccessLevel sessionAccessLevel, bool hidden)
    {
        return sessionAccessLevel switch
        {
            SessionAccessLevel.Private => hidden
                ? WorldPermissionType.PrivateHidden
                : WorldPermissionType.PrivateNonHidden,
            SessionAccessLevel.LAN => hidden
                ? WorldPermissionType.LanHidden
                : WorldPermissionType.LanNonHidden,
            SessionAccessLevel.Contacts => hidden
                ? WorldPermissionType.ContactsHidden
                : WorldPermissionType.ContactsNonHidden,
            SessionAccessLevel.ContactsPlus => hidden
                ? WorldPermissionType.ContactsPlusHidden
                : WorldPermissionType.ContactsPlusNonHidden,
            SessionAccessLevel.RegisteredUsers => hidden
                ? WorldPermissionType.RegisteredUsersHidden
                : WorldPermissionType.RegisteredUsersNonHidden,
            SessionAccessLevel.Anyone => hidden
                ? WorldPermissionType.AnyoneHidden
                : WorldPermissionType.AnyoneNonHidden,
            _ => throw new ArgumentOutOfRangeException(nameof(sessionAccessLevel), sessionAccessLevel, null)
        };
    }
}