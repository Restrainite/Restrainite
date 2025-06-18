using System;
using System.Collections.Generic;
using System.Linq;
using FrooxEngine;
using Restrainite.RestrictionTypes.Base;

namespace Restrainite;

internal class DynamicVariableSpaceSync : IDynamicVariableSpaceSync
{
    internal const string DynamicVariableSpaceName = "Restrainite";
    private const string TargetUserVariableName = "Target User";
    private const string PasswordVariableName = "Password";

    private static readonly List<DynamicVariableSpaceSync> Spaces = [];

    private readonly WeakReference<DynamicVariableSpace> _dynamicVariableSpace;

    private readonly DynamicVariableChangeListener<string> _passwordListener;

    private readonly DynamicVariableChangeListener<User> _targetUserListener;

    private ILocalRestriction[]? _restrictions;

    private DynamicVariableSpaceSync(DynamicVariableSpace dynamicVariableSpace)
    {
        _dynamicVariableSpace = new WeakReference<DynamicVariableSpace>(dynamicVariableSpace);
        _passwordListener =
            new DynamicVariableChangeListener<string>(dynamicVariableSpace, PasswordVariableName);
        _targetUserListener =
            new DynamicVariableChangeListener<User>(dynamicVariableSpace, TargetUserVariableName);
    }

    public bool IsActiveForLocalUser(IRestriction restriction)
    {
        if (!GetDynamicVariableSpace(out var dynamicVariableSpace)) return false;
        if (!IsValidRestrainiteDynamicSpace(dynamicVariableSpace) ||
            !RestrainiteMod.Configuration.AllowRestrictionsFromWorld(dynamicVariableSpace.World, restriction))
            return false;
        var user = _targetUserListener.DynamicValue;
        if (user != dynamicVariableSpace.LocalUser) return false;

        if (!RestrainiteMod.Configuration.RequiresPassword) return true;
        var password = _passwordListener.DynamicValue;
        return RestrainiteMod.Configuration.IsCorrectPassword(password);
    }

    private bool GetDynamicVariableSpace(out DynamicVariableSpace dynamicVariableSpace)
    {
        return _dynamicVariableSpace.TryGetTarget(out dynamicVariableSpace);
    }

    private bool Equals(DynamicVariableSpace dynamicVariableSpace)
    {
        var found = _dynamicVariableSpace.TryGetTarget(out var internalDynamicVariableSpace);
        return found && internalDynamicVariableSpace != null &&
               internalDynamicVariableSpace == dynamicVariableSpace;
    }

    internal static void UpdateList(DynamicVariableSpace dynamicVariableSpace)
    {
        var isValid = IsValidRestrainiteDynamicSpace(dynamicVariableSpace);
        DynamicVariableSpaceSync? shouldUnregister = null;
        DynamicVariableSpaceSync? shouldRegister = null;

        lock (Spaces)
        {
            var index = Spaces.FindIndex(space => space.Equals(dynamicVariableSpace));
            if (index != -1)
            {
                if (isValid) return;

                shouldUnregister = Spaces[index];
                Spaces.RemoveAt(index);
            }
            else
            {
                if (!isValid) return;

                shouldRegister = new DynamicVariableSpaceSync(dynamicVariableSpace);
                Spaces.Add(shouldRegister);
                dynamicVariableSpace.Destroyed += _ => { Remove(dynamicVariableSpace); };
            }
        }

        shouldUnregister?.Unregister();
        shouldRegister?.Register();
    }

    private void Register()
    {
        RestrainiteMod.Configuration.ShouldRecheckPermissions += ShouldCheckStates;
        _passwordListener.Register(OnPasswordChanged);
        _targetUserListener.Register(OnTargetUserChanged);
        if (!GetDynamicVariableSpace(out var dynamicVariableSpace)) return;
        _restrictions = Restrictions.All
            .Select(restriction => restriction.CreateLocal(dynamicVariableSpace, this))
            .ToArray();
    }

    private void OnTargetUserChanged(User user)
    {
        ShouldCheckStates();
    }

    private void OnPasswordChanged(string password)
    {
        ShouldCheckStates();
    }

    private void Unregister()
    {
        RestrainiteMod.Configuration.ShouldRecheckPermissions -= ShouldCheckStates;

        _passwordListener.Unregister(OnPasswordChanged);
        _targetUserListener.Unregister(OnTargetUserChanged);
        if (_restrictions == null) return;
        foreach (var restriction in _restrictions) restriction.Destroy();
        _restrictions = null;
    }

    private void ShouldCheckStates()
    {
        if (_restrictions == null) return;
        foreach (var restriction in _restrictions) restriction.Check();
    }

    private static bool IsValidRestrainiteDynamicSpace(DynamicVariableSpace dynamicVariableSpace)
    {
        return dynamicVariableSpace is { IsDestroyed: false, IsDisposed: false, CurrentName: DynamicVariableSpaceName };
    }

    internal static void Remove(DynamicVariableSpace dynamicVariableSpace)
    {
        DynamicVariableSpaceSync? dynamicVariableSpaceToRemove = null;
        lock (Spaces)
        {
            var index = Spaces.FindIndex(space => space.Equals(dynamicVariableSpace));
            if (index != -1)
            {
                dynamicVariableSpaceToRemove = Spaces[index];
                Spaces.RemoveAt(index);
            }
        }

        dynamicVariableSpaceToRemove?.Unregister();
    }
}