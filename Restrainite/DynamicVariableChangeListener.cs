using System;
using System.Collections.Generic;
using Elements.Core;
using FrooxEngine;
using ResoniteModLoader;
using Restrainite.RestrictionTypes.Base;

namespace Restrainite;

internal static class DynamicVariableChangeListener
{
    internal static bool HasShownWarning;
}

internal class DynamicVariableChangeListener<TV>(
    DynamicVariableSpace space,
    string variableName) : IDynamicVariable<TV>
{
    private readonly string _refId =
        $"Dynamic Variable Space {space.ReferenceID} " +
        $"created by {space.World.GetUserByAllocationID(space.ReferenceID.User)?.UserID} " +
        $"in {space.World.Name}";

    private TV? _value;

    public void ChildChanged(IWorldElement child)
    {
        var ex = new InvalidOperationException(
            "Invalid call to DynamicVariableChangeListener.ChildChanged(), this should never be called.");
        WarnInvalidCalls(ex);
        throw ex;
    }

    public DataTreeNode Save(SaveControl control)
    {
        var ex = new InvalidOperationException(
            "Invalid call to DynamicVariableChangeListener.Save(), this should never be called.");
        WarnInvalidCalls(ex);
        throw ex;
    }

    public void Load(DataTreeNode node, LoadControl control)
    {
        var ex = new InvalidOperationException(
            "Invalid call to DynamicVariableChangeListener.Load(), this should never be called.");
        WarnInvalidCalls(ex);
        throw ex;
    }

    public string GetSyncMemberName(ISyncMember member)
    {
        var ex = new InvalidOperationException(
            "Invalid call to DynamicVariableChangeListener.GetSyncMemberName(), this should never be called.");
        WarnInvalidCalls(ex);
        throw ex;
    }

    public RefID ReferenceID
    {
        get
        {
            var ex = new InvalidOperationException(
                "Invalid call to DynamicVariableChangeListener.ReferenceID, this should never be called.");
            WarnInvalidCalls(ex);
            throw ex;
        }
    }

    public string Name
    {
        get
        {
            var ex = new InvalidOperationException(
                "Invalid call to DynamicVariableChangeListener.Name, this should never be called.");
            WarnInvalidCalls(ex);
            throw ex;
        }
    }

    public World World => space.World;
    public IWorldElement Parent => space;
    public bool IsLocalElement => true;
    public bool IsPersistent => false;
    public bool IsRemoved => false;

    public void MarkSpaceDirty()
    {
        // Intentionally left blank, used by FrooxEngine.DynamicVariableSpace.ValueManager[T].Dispose
    }

    public bool UpdateLinking()
    {
        var ex = new InvalidOperationException(
            "Invalid call to DynamicVariableChangeListener.UpdateLinking(), this should never be called.");
        WarnInvalidCalls(ex);
        throw ex;
    }

    public string VariableName => variableName;

    public bool AlwaysOverrideOnLink => false;
    public bool IsWriteOnly => true;

    public TV DynamicValue
    {
        get => _value!;
        set
        {
            if (EqualityComparer<TV>.Default.Equals(_value!, value))
                return;
            _value = value;
            OnChange.SafeInvoke(value!);
        }
    }

    private static void WarnInvalidCalls(Exception ex)
    {
        if (DynamicVariableChangeListener.HasShownWarning) return;
        DynamicVariableChangeListener.HasShownWarning = true;
        ResoniteMod.Warn($"{RestrainiteMod.LogReportUrl} {ex.Message}: {ex}");
    }

    private event Action<TV>? OnChange;

    internal void Register(Action<TV> callback)
    {
        var manager = space.GetManager<TV>(VariableName, true);
        manager.Register(this);
        OnChange += callback;
    }

    internal void Unregister(Action<TV> callback)
    {
        var manager = space.GetManager<TV>(VariableName, true);
        manager.Unregister(this);
        OnChange -= callback;
    }

    internal void LogChange(string typeName, IRestriction restriction, TV value)
    {
        ResoniteMod.Msg(
            $"{typeName} of {restriction.Name} changed to '{value}'. ({_refId} @{space?.Slot?.GlobalPosition})");
    }
}