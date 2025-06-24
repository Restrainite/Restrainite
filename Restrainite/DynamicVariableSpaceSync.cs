using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using FrooxEngine;
using ResoniteModLoader;
using Restrainite.Enums;

namespace Restrainite;

internal class DynamicVariableSpaceSync
{
    internal const string DynamicVariableSpaceName = "Restrainite";
    private const string TargetUserVariableName = "Target User";
    private const string PasswordVariableName = "Password";

    private static readonly List<DynamicVariableSpaceSync> Spaces = [];

    private static readonly bool[] GlobalState = CreateDefaultArray(false);

    private static readonly float[] LowestFloatState = CreateDefaultArray(float.NaN);

    private static readonly IImmutableSet<string>[] GlobalStringSet =
        CreateDefaultArray<IImmutableSet<string>>(ImmutableHashSet<string>.Empty);

    private readonly DynamicVariableKeyChangeListener<PreventionType, bool>[]
        _boolListeners = CreateDefaultArray<DynamicVariableKeyChangeListener<PreventionType, bool>>(null!);

    private readonly WeakReference<DynamicVariableSpace> _dynamicVariableSpace;

    private readonly DynamicVariableKeyChangeListener<PreventionType, float>?[] _floatListeners =
        CreateDefaultArray<DynamicVariableKeyChangeListener<PreventionType, float>?>(null!);

    private readonly float[] _localFloatValues = CreateDefaultArray(float.NaN);

    private readonly bool[] _localState = CreateDefaultArray(false);

    private readonly DynamicVariableChangeListener<string> _passwordListener;

    private readonly string _refId;

    private readonly string[][] _stringListValues =
        CreateDefaultArray<string[]>([]);

    private readonly DynamicVariableKeyChangeListener<PreventionType, string>?[] _stringSetListeners =
        CreateDefaultArray<DynamicVariableKeyChangeListener<PreventionType, string>?>(null!);

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
        foreach (var preventionType in PreventionTypes.List)
        {
            _boolListeners[(int)preventionType] = CreateListener<bool>(dynamicVariableSpace, preventionType);
            if (preventionType.IsFloatType())
                _floatListeners[(int)preventionType] = CreateListener<float>(dynamicVariableSpace, preventionType);

            if (preventionType.IsStringSetType())
                _stringSetListeners[(int)preventionType] = CreateListener<string>(dynamicVariableSpace, preventionType);
        }
    }

    private static T[] CreateDefaultArray<T>(T defaultValue)
    {
        var array = new T[PreventionTypes.Max];
        for (var i = 0; i < PreventionTypes.Max; i++) array[i] = defaultValue;
        return array;
    }

    private static DynamicVariableKeyChangeListener<PreventionType, T> CreateListener<T>(
        DynamicVariableSpace dynamicVariableSpace, PreventionType preventionType)
    {
        return new DynamicVariableKeyChangeListener<PreventionType, T>(
            dynamicVariableSpace,
            preventionType,
            preventionType.ToExpandedString());
    }

    private bool Equals(DynamicVariableSpace dynamicVariableSpace)
    {
        var found = _dynamicVariableSpace.TryGetTarget(out var internalDynamicVariableSpace);
        return found && internalDynamicVariableSpace != null &&
               internalDynamicVariableSpace == dynamicVariableSpace;
    }

    private bool GetLocalState(PreventionType preventionType)
    {
        return _localState[(int)preventionType];
    }

    private float GetLocalFloat(PreventionType preventionType)
    {
        return _localFloatValues[(int)preventionType];
    }

    private IEnumerable<string> GetLocalStringSet(PreventionType preventionType)
    {
        return _stringListValues[(int)preventionType];
    }

    private void UpdateLocalState(PreventionType preventionType, bool value)
    {
        UpdateLocalStateInternal(preventionType, IsActiveForLocalUser(preventionType) && value);
    }

    private void UpdateLocalFloatState(PreventionType preventionType, float value)
    {
        if (!preventionType.IsFloatType()) return;
        var currentValue = _localFloatValues[(int)preventionType];
        if ((float.IsNaN(currentValue) && float.IsNaN(value)) ||
            (float.IsPositiveInfinity(currentValue) && float.IsPositiveInfinity(value)) ||
            (float.IsNegativeInfinity(currentValue) && float.IsNegativeInfinity(value)) ||
            currentValue == value) return;
        _localFloatValues[(int)preventionType] = value;
        UpdateGlobalState(preventionType, "");
    }

    private void UpdateLocalStringListState(PreventionType preventionType, string value)
    {
        if (!preventionType.IsStringSetType()) return;
        var currentValue = _stringListValues[(int)preventionType];
        var newValue = SplitStringToList(value);
        if (currentValue.Length == newValue.Length && currentValue.SequenceEqual(newValue)) return;
        _stringListValues[(int)preventionType] = newValue;
        UpdateGlobalState(preventionType, "");
    }

    private void UpdateLocalStateInternal(PreventionType preventionType, bool value)
    {
        if (_localState[(int)preventionType] == value) return;
        _localState[(int)preventionType] = value;
        var source = Source();
        ResoniteMod.Msg($"Local State of {preventionType.ToExpandedString()} changed to {value}. ({source})");

        UpdateGlobalState(preventionType, source);
    }

    private void UpdateGlobalState(PreventionType preventionType, string source)
    {
        var globalState = CalculateGlobalState(preventionType);

        if (GetGlobalState(preventionType) != globalState)
        {
            GlobalState[(int)preventionType] = globalState;
            ResoniteMod.Msg($"Global State of {preventionType.ToExpandedString()} " +
                            $"changed to {globalState}. ({source})");
            NotifyGlobalStateChange(preventionType, globalState);
        }

        if (preventionType.IsFloatType())
        {
            var lowestFloat = CalculateLowestFloatState(preventionType);
            var currentValue = GetLowestGlobalFloat(preventionType);
            if (float.IsNaN(currentValue) && float.IsNaN(lowestFloat)) return;
            if (float.IsPositiveInfinity(currentValue) && float.IsPositiveInfinity(lowestFloat)) return;
            if (float.IsNegativeInfinity(currentValue) && float.IsNegativeInfinity(lowestFloat)) return;
            if (currentValue == lowestFloat) return;
            LowestFloatState[(int)preventionType] = lowestFloat;
            NotifyGlobalFloatStateChange(preventionType, lowestFloat);
        }

        if (preventionType.IsStringSetType())
        {
            var completeSet = CalculateStringSet(preventionType);
            var currentValue = GetGlobalStringSet(preventionType);
            if (currentValue.Equals(completeSet)) return;
            GlobalStringSet[(int)preventionType] = completeSet;
            NotifyGlobalStringSetChange(preventionType, completeSet);
        }
    }

    private string Source()
    {
        var found = GetDynamicVariableSpace(out var internalDynamicVariableSpace);
        if (!found) return _refId;

        var slot = internalDynamicVariableSpace?.Slot;

        if (slot == null) return _refId;

        var slotTree = new StringBuilder();
        var parent = slot.Parent;
        var maxDepth = 20;
        while (parent != null && maxDepth-- > 0)
        {
            slotTree.Insert(0, $"/{parent.Name}");
            parent = parent.Parent;
        }

        return $"{_refId} {slotTree}";
    }

    private static bool CalculateGlobalState(PreventionType preventionType)
    {
        bool globalState;
        lock (Spaces)
        {
            globalState = Spaces.FindIndex(space => space.GetLocalState(preventionType)) != -1;
        }

        return globalState;
    }

    private static float CalculateLowestFloatState(PreventionType preventionType)
    {
        var globalState = float.NaN;
        lock (Spaces)
        {
            foreach (var space in Spaces)
            {
                if (!space.GetLocalState(preventionType)) continue;
                var local = space.GetLocalFloat(preventionType);
                if (float.IsNaN(local)) continue;
                if (float.IsNaN(globalState) || local < globalState) globalState = local;
            }
        }

        return globalState;
    }

    private static IImmutableSet<string> CalculateStringSet(PreventionType preventionType)
    {
        List<DynamicVariableSpaceSync> spaces;
        lock (Spaces)
        {
            spaces = Spaces.Where(space => space.GetLocalState(preventionType)).ToList();
        }

        return spaces.SelectMany(space => space.GetLocalStringSet(preventionType)).ToImmutableHashSet();
    }

    private void NotifyGlobalStateChange(PreventionType preventionType, bool value)
    {
        if (!GetDynamicVariableSpace(out var dynamicVariableSpace)) return;
        RestrainiteMod.NotifyRestrictionChanged(dynamicVariableSpace.World, preventionType, value);
    }

    private void NotifyGlobalFloatStateChange(PreventionType preventionType, float value)
    {
        if (!GetDynamicVariableSpace(out var dynamicVariableSpace)) return;
        RestrainiteMod.NotifyFloatChanged(dynamicVariableSpace.World, preventionType, value);
    }

    private void NotifyGlobalStringSetChange(PreventionType preventionType, IImmutableSet<string> value)
    {
        if (!GetDynamicVariableSpace(out var dynamicVariableSpace)) return;
        RestrainiteMod.NotifyStringSetChanged(dynamicVariableSpace.World, preventionType, value);
    }

    private void UpdateAllGlobalStates()
    {
        var source = Source();
        foreach (var preventionType in PreventionTypes.List) UpdateGlobalState(preventionType, source);
    }

    internal static bool GetGlobalState(PreventionType preventionType)
    {
        return GlobalState[(int)preventionType];
    }

    public static float GetLowestGlobalFloat(PreventionType preventionType)
    {
        return LowestFloatState[(int)preventionType];
    }

    internal static IImmutableSet<string> GetGlobalStringSet(PreventionType preventionType)
    {
        return GlobalStringSet[(int)preventionType];
    }

    private static string[] SplitStringToList(object? value)
    {
        var splitArray = (value as string)?.Split(',') ?? [];
        return splitArray.Select(t => t.Trim())
            .Where(trimmed => trimmed.Length != 0)
            .ToArray();
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
        RestrainiteMod.Configuration.ShouldRecheckPermissions += ShouldRecheckPermissions;
        _passwordListener.Register(OnPasswordChanged);
        _targetUserListener.Register(OnTargetUserChanged);
        foreach (var preventionType in PreventionTypes.List)
        {
            _boolListeners[(int)preventionType].Register(UpdateLocalState);
            if (preventionType.IsFloatType()) _floatListeners[(int)preventionType]?.Register(UpdateLocalFloatState);
            if (preventionType.IsStringSetType())
                _stringSetListeners[(int)preventionType]?.Register(UpdateLocalStringListState);
        }

        UpdateAllGlobalStates();
    }

    private void OnTargetUserChanged(User user)
    {
        CheckLocalState();
    }

    private void OnPasswordChanged(string password)
    {
        CheckLocalState();
    }

    private void Unregister()
    {
        RestrainiteMod.Configuration.ShouldRecheckPermissions -= ShouldRecheckPermissions;

        _passwordListener.Unregister(OnPasswordChanged);
        _targetUserListener.Unregister(OnTargetUserChanged);
        foreach (var preventionType in PreventionTypes.List)
        {
            _boolListeners[(int)preventionType].Unregister(UpdateLocalState);
            if (preventionType.IsFloatType()) _floatListeners[(int)preventionType]?.Unregister(UpdateLocalFloatState);
            if (preventionType.IsStringSetType())
                _stringSetListeners[(int)preventionType]?.Unregister(UpdateLocalStringListState);
        }

        UpdateAllGlobalStates();
    }

    private void ShouldRecheckPermissions()
    {
        CheckLocalState();
    }

    private void CheckLocalState()
    {
        foreach (var preventionType in PreventionTypes.List)
            CheckLocalState(preventionType);
    }

    private void CheckLocalState(PreventionType preventionType)
    {
        var state = IsActiveForLocalUser(preventionType);
        if (state) state = _boolListeners[(int)preventionType].DynamicValue;

        if (_localState[(int)preventionType] != state) UpdateLocalStateInternal(preventionType, state);
    }

    private bool GetDynamicVariableSpace(out DynamicVariableSpace dynamicVariableSpace)
    {
        return _dynamicVariableSpace.TryGetTarget(out dynamicVariableSpace);
    }

    private static bool IsValidRestrainiteDynamicSpace(DynamicVariableSpace dynamicVariableSpace)
    {
        return dynamicVariableSpace is { IsDestroyed: false, IsDisposed: false, CurrentName: DynamicVariableSpaceName };
    }

    private bool IsActiveForLocalUser(PreventionType preventionType)
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

    public static void Remove(DynamicVariableSpace dynamicVariableSpace)
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

        dynamicVariableSpaceToRemove?.UpdateAllGlobalStates();
    }
}