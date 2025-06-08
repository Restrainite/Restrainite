using System;
using System.Collections.Generic;
using System.Linq;
using FrooxEngine;
using Restrainite.Enums;
using Restrainite.States;

namespace Restrainite;

internal class DynamicVariableSpaceSync : IDynamicVariableSpaceWrapper
{
    internal const string DynamicVariableSpaceName = "Restrainite";
    private const string TargetUserVariableName = "Target User";
    private const string PasswordVariableName = "Password";

    private static readonly List<DynamicVariableSpaceSync> Spaces = [];

    private readonly WeakReference<DynamicVariableSpace> _dynamicVariableSpace;

    private readonly LocalStateBool _localBoolState;

    private readonly LocalStateLowestFloat _localLowestFloatState;

    private readonly LocalStateStringSet _localStringSetState;

    private readonly DynamicVariableChangeListener<string> _passwordListener;

    private readonly string _refId;

    private readonly DynamicVariableChangeListener<User> _targetUserListener;

    private DynamicVariableSpaceSync(DynamicVariableSpace dynamicVariableSpace)
    {
        _dynamicVariableSpace = new WeakReference<DynamicVariableSpace>(dynamicVariableSpace);
        var user = dynamicVariableSpace.World.GetUserByAllocationID(dynamicVariableSpace.ReferenceID.User);
        _refId = $"Dynamic Variable Space {dynamicVariableSpace.ReferenceID} created by {user?.UserID} " +
                 $"in {dynamicVariableSpace.World?.Name}";
        _passwordListener =
            new DynamicVariableChangeListener<string>(dynamicVariableSpace, PasswordVariableName);
        _targetUserListener =
            new DynamicVariableChangeListener<User>(dynamicVariableSpace, TargetUserVariableName);
        _localBoolState = new LocalStateBool(this);
        _localLowestFloatState = new LocalStateLowestFloat(this);
        _localStringSetState = new LocalStateStringSet(this);
    }

    public float GetLowestFloatState(PreventionType preventionType)
    {
        return _localLowestFloatState.Get(preventionType);
    }

    public ImmutableStringSet GetStringSetState(PreventionType preventionType)
    {
        return _localStringSetState.Get(preventionType);
    }

    public bool GetDynamicVariableSpace(out DynamicVariableSpace dynamicVariableSpace)
    {
        return _dynamicVariableSpace.TryGetTarget(out dynamicVariableSpace);
    }

    public bool IsActiveForLocalUser(PreventionType preventionType)
    {
        if (!GetDynamicVariableSpace(out var dynamicVariableSpace)) return false;
        if (!IsValidRestrainiteDynamicSpace(dynamicVariableSpace) ||
            !RestrainiteMod.Configuration.AllowRestrictionsFromWorld(dynamicVariableSpace.World, preventionType))
            return false;
        var user = _targetUserListener.DynamicValue;
        if (user != dynamicVariableSpace.LocalUser) return false;

        if (!RestrainiteMod.Configuration.RequiresPassword) return true;
        var password = _passwordListener.DynamicValue;
        return RestrainiteMod.Configuration.IsCorrectPassword(password);
    }

    public ICollection<IDynamicVariableSpaceWrapper> GetActiveSpaces(PreventionType preventionType)
    {
        lock (Spaces)
        {
            return Spaces.Where(space => space._localBoolState.Get(preventionType)).ToArray();
        }
    }

    private bool Equals(DynamicVariableSpace dynamicVariableSpace)
    {
        var found = _dynamicVariableSpace.TryGetTarget(out var internalDynamicVariableSpace);
        return found && internalDynamicVariableSpace != null &&
               internalDynamicVariableSpace == dynamicVariableSpace;
    }

    public override string ToString()
    {
        var found = GetDynamicVariableSpace(out var internalDynamicVariableSpace);
        return found ? $"{_refId} @{internalDynamicVariableSpace?.Slot?.GlobalPosition}" : _refId;
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
        _localBoolState.Register();
        _localLowestFloatState.Register();
        _localStringSetState.Register();
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
        _localBoolState.Unregister();
        _localLowestFloatState.Unregister();
        _localStringSetState.Unregister();
    }

    private void ShouldCheckStates()
    {
        _localBoolState.CheckState();
        _localLowestFloatState.CheckState();
        _localStringSetState.CheckState();
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