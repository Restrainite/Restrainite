using System;
using System.Collections.Generic;
using Elements.Core;
using FrooxEngine;
using ResoniteModLoader;

namespace Restrainite;

internal static class DynamicVariableChangeListener
{
    internal static bool HasShownWarning;
}

internal class DynamicVariableChangeListener<TV> : IDynamicVariable<TV>
{
    private readonly WeakReference<DynamicVariableSpace> _space;

    private TV? _value;

    internal DynamicVariableChangeListener(DynamicVariableSpace space, string variableName, Action<TV> callback)
    {
        _space = new WeakReference<DynamicVariableSpace>(space);
        VariableName = variableName;
        OnChange += callback;
        var manager = space.GetManager<TV>(VariableName, true);
        manager.Register(this);
    }

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

    public World World => !_space.TryGetTarget(out var dynamicVariableSpace) ? null! : dynamicVariableSpace.World;

    public IWorldElement Parent => !_space.TryGetTarget(out var dynamicVariableSpace) ? null! : dynamicVariableSpace;
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

    public string VariableName { get; }

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

    internal void Unregister(Action<TV> callback)
    {
        OnChange -= callback;
        if (!_space.TryGetTarget(out var space)) return;
        var manager = space.GetManager<TV>(VariableName, true);
        manager.Unregister(this);
    }
}