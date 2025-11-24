using System.Text;
using FrooxEngine;
using Restrainite.RestrictionTypes.Base;

namespace Restrainite;

internal sealed class DynamicVariableSpaceSync : IDynamicVariableSpace
{
    internal const string DynamicVariableSpaceName = "Restrainite";
    private const string TargetUserVariableName = "Target User";
    private const string PasswordVariableName = "Password";

    private static readonly List<DynamicVariableSpaceSync> Spaces = [];

    private readonly WeakReference<DynamicVariableSpace> _dynamicVariableSpace;

    private readonly DynamicVariableChangeListener<string> _passwordListener;

    private readonly string _refId;

    private readonly LocalRestriction[] _restrictions;

    private readonly DynamicVariableChangeListener<User> _targetUserListener;
    private volatile string _cachedSource;
    private long _tick;

    private DynamicVariableSpaceSync(DynamicVariableSpace dynamicVariableSpace)
    {
        _refId = $"Dynamic Variable Space {dynamicVariableSpace.ReferenceID} " +
                 $"created by {dynamicVariableSpace.World.GetUserByAllocationID(dynamicVariableSpace.ReferenceID.User)?.UserID} " +
                 $"in {dynamicVariableSpace.World.Name}";
        _cachedSource = _refId;
        _dynamicVariableSpace = new WeakReference<DynamicVariableSpace>(dynamicVariableSpace);
        _passwordListener =
            new DynamicVariableChangeListener<string>(dynamicVariableSpace, PasswordVariableName, OnPasswordChanged);
        _targetUserListener =
            new DynamicVariableChangeListener<User>(dynamicVariableSpace, TargetUserVariableName, OnTargetUserChanged);
        RestrainiteMod.Configuration.ShouldRecheckPermissions += ShouldCheckStates;
        _restrictions = Restrictions.All
            .Select(restriction => restriction.CreateLocal(dynamicVariableSpace, this))
            .ToArray();
    }

    public bool IsActiveForLocalUser(IRestriction restriction)
    {
        if (!GetDynamicVariableSpace(out var dynamicVariableSpace) || dynamicVariableSpace == null) return false;
        if (!IsValidRestrainiteDynamicSpace(dynamicVariableSpace) ||
            !RestrainiteMod.Configuration.AllowRestrictionsFromWorld(dynamicVariableSpace.World, restriction))
            return false;
        var user = _targetUserListener.DynamicValue;
        if (user != dynamicVariableSpace.LocalUser) return false;

        if (!RestrainiteMod.Configuration.RequiresPassword) return true;
        var password = _passwordListener.DynamicValue;
        return RestrainiteMod.Configuration.IsCorrectPassword(password);
    }

    public string AsString()
    {
        var tick = Engine.Current.UpdateTick;
        if (tick == Interlocked.Read(ref _tick)) return _cachedSource;
        if (!_dynamicVariableSpace.TryGetTarget(out var space))
        {
            Interlocked.Exchange(ref _cachedSource, _refId);
            Interlocked.Exchange(ref _tick, tick);
            return _refId;
        }

        var slot = space?.Slot;

        if (slot == null)
        {
            Interlocked.Exchange(ref _cachedSource, _refId);
            Interlocked.Exchange(ref _tick, tick);
            return _refId;
        }

        var slotTree = new StringBuilder();
        slotTree.Append(slot.Name).Append('/');
        var parent = slot.Parent;
        var maxDepth = 20;
        while (parent != null && maxDepth-- > 0)
        {
            slotTree.Append(parent.Name).Append('/');
            parent = parent.Parent;
        }

        if (maxDepth <= 0) slotTree.Append("...");

        var source = $"{_refId} at {slotTree}";
        Interlocked.Exchange(ref _cachedSource, source);
        Interlocked.Exchange(ref _tick, tick);
        return source;
    }


    private bool GetDynamicVariableSpace(out DynamicVariableSpace? dynamicVariableSpace)
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

                var dynamicVariableSpaceSync = new DynamicVariableSpaceSync(dynamicVariableSpace);
                Spaces.Add(dynamicVariableSpaceSync);
                dynamicVariableSpace.Destroyed += dynamicVariableSpaceSync.OnSpaceDestroyed;
            }
        }

        shouldUnregister?.Unregister();
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

        _passwordListener.Unregister();
        _targetUserListener.Unregister();
        foreach (var restriction in _restrictions) restriction.Destroy();
    }

    private void ShouldCheckStates()
    {
        foreach (var restriction in _restrictions) restriction.Check();
    }

    private static bool IsValidRestrainiteDynamicSpace(DynamicVariableSpace dynamicVariableSpace)
    {
        return dynamicVariableSpace is { IsDestroyed: false, IsDisposed: false, CurrentName: DynamicVariableSpaceName };
    }

    private void OnSpaceDestroyed(IDestroyable destroyable)
    {
        destroyable.Destroyed -= OnSpaceDestroyed;
        lock (Spaces)
        {
            Spaces.Remove(this);
        }

        Unregister();
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