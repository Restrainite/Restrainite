using System;
using Elements.Core;
using FrooxEngine;
using ResoniteModLoader;
using Restrainite.Enums;
using Restrainite.RestrictionTypes.Base;

namespace Restrainite;

internal class RestrictionStateOutput
{
    private const string RestrainiteRootSlotName = "Restrainite Status";
    private const string DynamicVariableSpaceStatusName = "Restrainite Status";
    private readonly Configuration _configuration;
    private readonly WeakReference<Slot> _userSlot;
    private bool _isBeingShown;
    private WeakReference<Slot>? _oldSlot;

    internal RestrictionStateOutput(Configuration configuration, Slot userSlot)
    {
        _configuration = configuration;
        _userSlot = new WeakReference<Slot>(userSlot);

        ShowOrHideRestrainiteRootSlot();
    }

    internal void OnShouldRecheckPermissions()
    {
        if (!_userSlot.TryGetTarget(out var userSlot)) return;
        if (userSlot.IsDestroyed || userSlot.IsDestroying) return;
        userSlot.RunSynchronously(ShowOrHideRestrainiteRootSlot);
    }

    private void ShowOrHideRestrainiteRootSlot()
    {
        if (!_userSlot.TryGetTarget(out var userSlot)) return;
        var show = _configuration.AllowRestrictionsFromWorld(userSlot.World) ||
                   userSlot.World == Userspace.UserspaceWorld;
        if (!show && !_isBeingShown) return;
        _isBeingShown = show;

        userSlot.RunSynchronously(show ? AddRestrainiteSlot : RemoveRestrainiteSlot);
    }

    private void AddRestrainiteSlot()
    {
        if (!_userSlot.TryGetTarget(out var userSlot)) return;
        if (userSlot.IsDestroyed || userSlot.IsDestroying) return;
        CreateDynamicVariableSpace();

        ResoniteMod.Msg($"Adding Restrainite slot to {userSlot.Name} {userSlot.ReferenceID} " +
                        $"in {userSlot.Parent?.Name} {userSlot.World?.Name}");
        DeleteOldSlotIfMovedOutOfUserSlot(userSlot);
        var restrainiteSlot = _oldSlot != null && _oldSlot.TryGetTarget(out var slot)
            ? slot
            : userSlot.FindChildOrAdd(RestrainiteRootSlotName, false);
        _oldSlot = new WeakReference<Slot>(restrainiteSlot);

        CreateVersionComponent(restrainiteSlot);

        if (restrainiteSlot.World == Userspace.UserspaceWorld) CreatePresetComponent(restrainiteSlot);

        CreatePasswordComponent(restrainiteSlot);

        AddSelfReferenceComponent(restrainiteSlot);

        AddOrRemoveComponents(restrainiteSlot);
    }

    private void DeleteOldSlotIfMovedOutOfUserSlot(Slot userSlot)
    {
        if (_oldSlot == null ||
            !_oldSlot.TryGetTarget(out var slot) ||
            slot.FindParent(s => s == userSlot, 20) != null) return;
        slot.Destroy(true);
        _oldSlot = null;
    }

    private void CreateDynamicVariableSpace()
    {
        if (!_userSlot.TryGetTarget(out var userSlot)) return;
        ResoniteMod.Msg($"Adding Restrainite DynamicVariableSpace to {userSlot.Name} {userSlot.ReferenceID} " +
                        $"in {userSlot.Parent?.Name} {userSlot.World?.Name}");
        var dynamicVariableSpace = userSlot.GetComponentOrAttach<DynamicVariableSpace>(component =>
            DynamicVariableSpaceStatusName.Equals(component.CurrentName)
        );
        dynamicVariableSpace.OnlyDirectBinding.Value = true;
        dynamicVariableSpace.SpaceName.Value = DynamicVariableSpaceStatusName;
        dynamicVariableSpace.Persistent = false;
    }

    private void RemoveRestrainiteSlot()
    {
        if (_userSlot.TryGetTarget(out var userSlot) &&
            !userSlot.IsDestroyed &&
            !userSlot.IsDestroying)
        {
            userSlot.RemoveAllComponents(component => component is DynamicVariableSpace
            {
                CurrentName: DynamicVariableSpaceStatusName
            });

            var restrainiteSlot = userSlot.FindChild(RestrainiteRootSlotName);
            restrainiteSlot?.Destroy(true);
        }

        if (_oldSlot == null || !_oldSlot.TryGetTarget(out var slot)) return;
        if (slot.IsDestroying || slot.IsDestroyed) return;
        slot.Destroy(true);
    }

    private void AddOrRemoveComponents(Slot restrainiteSlot)
    {
        foreach (var restriction in Restrictions.All)
            AddOrRemoveComponents(restrainiteSlot, restriction);
    }

    private void AddOrRemoveComponents(Slot restrainiteSlot, IRestriction restriction)
    {
        if (_configuration.IsRestrictionEnabled(restriction))
            CreateComponents(restrainiteSlot, restriction);
        else
            RemoveComponents(restrainiteSlot, restriction);
    }

    private static void CreateVersionComponent(Slot restrainiteSlot)
    {
        const string versionName = $"{DynamicVariableSpaceStatusName}/Version";
        var component =
            restrainiteSlot.GetComponentOrAttach<DynamicValueVariable<uint3>>(search =>
                versionName.Equals(search.VariableName.Value));
        component.VariableName.Value = versionName;
        component.Persistent = false;
        var version = RestrainiteMod.AssemblyVersion;
        var versionArray = new uint3(
            version.Major < 0 ? 0 : (uint)version.Major,
            version.Minor < 0 ? 0 : (uint)version.Minor,
            version.Build < 0 ? 0 : (uint)version.Build);
        component.Value.Value = versionArray;
    }

    private void CreatePresetComponent(Slot restrainiteSlot)
    {
        const string presetName = $"{DynamicVariableSpaceStatusName}/Preset";
        var component = restrainiteSlot.GetComponentOrAttach<DynamicValueVariable<string>>(
            out var attached,
            search => presetName.Equals(search.VariableName.Value));
        component.VariableName.Value = presetName;
        component.Persistent = false;
        component.Value.Value = _configuration.CurrentPreset?.ToString() ?? "";

        if (!attached) return;
        component.Value.OnValueChange += OnPresetChanged;
    }

    private void CreatePasswordComponent(Slot restrainiteSlot)
    {
        const string passwordName = $"{DynamicVariableSpaceStatusName}/Requires Password";
        var component =
            restrainiteSlot.GetComponentOrAttach<DynamicValueVariable<bool>>(search =>
                passwordName.Equals(search.VariableName.Value));
        component.VariableName.Value = passwordName;
        component.Persistent = false;
        component.Value.Value = _configuration.RequiresPassword;
    }

    private static void AddSelfReferenceComponent(Slot restrainiteSlot)
    {
        const string selfReferenceName = "User/Restrainite Status Slot";
        var component =
            restrainiteSlot.GetComponentOrAttach<DynamicReferenceVariable<Slot>>(search =>
                selfReferenceName.Equals(search.VariableName.Value));
        component.VariableName.Value = selfReferenceName;
        component.Persistent = false;
        component.Reference.Target = restrainiteSlot;
    }

    private static void OnPresetChanged(SyncField<string> syncField)
    {
        try
        {
            var presetType = (PresetType)Enum.Parse(typeof(PresetType), syncField.Value);
            if ((int)presetType >= PresetTypes.Max || (int)presetType < 0) throw new OverflowException();
            RestrainiteMod.Configuration.CurrentPreset = presetType;
        }
        catch (Exception ex) when (ex is ArgumentNullException or ArgumentException or OverflowException)
        {
            syncField.Slot.RunInUpdates(0,
                () => syncField.Value = RestrainiteMod.Configuration.CurrentPreset?.ToString() ?? "");
        }
    }

    private static void CreateComponents(Slot restrainiteSlot, IRestriction restriction)
    {
        var slot = restrainiteSlot.FindChild(restriction.Name);
        if (slot == null)
        {
            slot = restrainiteSlot.AddSlot(restriction.Name, false);
        }

        slot.Tag = $"{DynamicVariableSpaceSync.DynamicVariableSpaceName}/{restriction.Name}";

        restriction.CreateStatusComponent(slot, DynamicVariableSpaceStatusName);
    }

    private static void RemoveComponents(Slot restrainiteSlot, IRestriction restriction)
    {
        var oldSlot = restrainiteSlot.FindChild(restriction.Name);

        if (oldSlot == null) return;
        if (oldSlot.IsDestroyed || oldSlot.IsDestroying) return;

        if (oldSlot.ChildrenCount != 0)
        {
            ResoniteMod.Warn($"Unable to remove slot {oldSlot.Name} {oldSlot.ReferenceID} in " +
                             $"{oldSlot.Parent?.Name} {oldSlot.World?.Name}, too many children: {oldSlot.ChildrenCount}");
            return;
        }

        ResoniteMod.Msg($"Removing Status Slot for {restriction.Name} in {restrainiteSlot.Name} " +
                        $"{restrainiteSlot.ReferenceID} in {restrainiteSlot.Parent?.Name} {restrainiteSlot.World?.Name}");

        oldSlot.Destroy(true);
    }
}