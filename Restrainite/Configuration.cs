using System.Collections;
using FrooxEngine;
using ResoniteModLoader;
using Restrainite.Enums;
using Restrainite.RestrictionTypes.Base;
using SkyFrost.Base;

namespace Restrainite;

internal sealed class Configuration
{
    private readonly ModConfigurationKey<bool> _allowRestrictionsFromFocusedWorldOnly = new(
        "Allow Restrictions from Focused World only",
        "Restrictions can only be modified from the focused world",
        () => true);

    private readonly Dictionary<WorldPermissionType, ModConfigurationKey<PresetChangeType>>
        _changeOnWorldPermissionChangeDict = new();

    private readonly ModConfigurationKey<bool>[] _displayedRestrictions =
        new ModConfigurationKey<bool>[Restrictions.Length];

    private readonly ModConfigurationKey<bool> _hasSeenAllPresetWarning = new(
        "Seen All preset warning",
        string.Empty, () => false, true);

    private readonly ModConfigurationKey<string> _password = new(
        "Password",
        "Select a password (not secret), that should be present in the Restrainite Dynamic Variable Space. ",
        () => string.Empty);

    private readonly ModConfigurationKey<PresetType> _presetConfig = new("Preset",
        "Select a preset",
        () => PresetType.None);

    private readonly ModConfigurationKey<Dictionary<string, string>> _presetPasswords = new(
        "Preset Passwords", string.Empty, () => [], true);

    private readonly ModConfigurationKey<PresetChangeType> _presetStartupConfig = new(
        "Preset on Startup",
        "Select a preset that should be loaded on game startup. DoNotChange will not change the preset on startup.",
        () => PresetChangeType.None);

    private readonly Dictionary<PresetType, ModConfigurationKey<Dictionary<string, bool>>> _presetStore = new();

    private readonly ModConfigurationKey<bool> _sendDynamicImpulses = new(
        "Send dynamic impulses",
        "Send a dynamic impulse to the user root slot every time a restriction is activated or deactivated.",
        () => true);

    private readonly ModConfigurationKey<bool> _setBusyStatus = new(
        "Set online status to busy",
        "When you are unable to reply to messages due to restrictions, automatically set online status to busy, if you are not invisible.",
        () => true);


    private ModConfiguration? _config;

    public Configuration()
    {
        foreach (var presetType in PresetTypes.List)
            _presetStore.Add(presetType, new ModConfigurationKey<Dictionary<string, bool>>(
                $"Preset{presetType}", "", () => new Dictionary<string, bool>(), true));
    }

    internal bool SendDynamicImpulses => _config?.GetValue(_sendDynamicImpulses) ?? true;

    internal bool SetBusyStatus => _config?.GetValue(_setBusyStatus) ?? true;

    internal PresetType? CurrentPreset
    {
        get => _config?.GetValue(_presetConfig);
        set
        {
            if (value != null && _config?.GetValue(_presetConfig) != value) _config?.Set(_presetConfig, value);
        }
    }

    internal bool RequiresPassword { get; private set; }

    internal event Action? ShouldRecheckPermissions;

    public void DefineConfiguration(ModConfigurationDefinitionBuilder builder)
    {
        ResoniteMod.Msg("Define configuration");
        builder.Version(RestrainiteMod.AssemblyVersion);
        builder.Key(_presetConfig);
        builder.Key(_presetStartupConfig);

        foreach (var key in _presetStore.Values) builder.Key(key);

        foreach (var restriction in Restrictions.All)
        {
            var key = new ModConfigurationKey<bool>($"Allow {restriction.Name} Restriction",
                restriction.Description, () => false);
            builder.Key(key);
            _displayedRestrictions[restriction.Index] = key;
        }

        builder.Key(_password);
        builder.Key(_presetPasswords);

        foreach (var worldPermissionType in WorldPermissionTypes.List)
        {
            var key = new ModConfigurationKey<PresetChangeType>(
                $"Change to Preset, if world permissions are {worldPermissionType.AsExpandedString()}",
                "", () => worldPermissionType.Default()
            );
            builder.Key(key);
            _changeOnWorldPermissionChangeDict.Add(worldPermissionType, key);
        }

        builder.Key(_allowRestrictionsFromFocusedWorldOnly);
        builder.Key(_sendDynamicImpulses);
        builder.Key(_setBusyStatus);
        builder.Key(_hasSeenAllPresetWarning);
    }

    public void Init(ModConfiguration? config)
    {
        _config = config;
        _presetConfig.OnChanged += OnPresetSelected;
        for (var index = 0; index < _displayedRestrictions.Length; index++)
        {
            var displayedRestriction = _displayedRestrictions[index];
            displayedRestriction.OnChanged += OnRestrictionConfigChanged(index);
        }

        var presetOnStartup = _config?.GetValue(_presetStartupConfig) ?? PresetChangeType.None;
        if (presetOnStartup != PresetChangeType.DoNotChange) _config?.Set(_presetConfig, (PresetType)presetOnStartup);

        foreach (var key in _presetStore.Values)
            key.OnChanged += ShouldRecheckPermissionInvoker;

        _password.OnChanged += UpdatePasswordState;

        foreach (var key in _changeOnWorldPermissionChangeDict.Values)
            key.OnChanged += ShouldRecheckPermissionInvoker;

        _presetConfig.OnChanged += ShouldRecheckPermissionInvoker;
        _allowRestrictionsFromFocusedWorldOnly.OnChanged += ShouldRecheckPermissionInvoker;

        _config?.Save(true);
    }

    private void ShouldRecheckPermissionInvoker(object? _)
    {
        ShouldRecheckPermissions.SafeInvoke();
    }

    private ModConfigurationKey.OnChangedHandler OnRestrictionConfigChanged(int restrictionIndex)
    {
        var restriction = Restrictions.All[restrictionIndex];
        return value =>
        {
            var boolValue = value as bool? ?? false;
            var presetType = _config?.GetValue(_presetConfig) ?? PresetType.Customized;
            switch (presetType)
            {
                case PresetType.None when !boolValue:
                    return;
                case PresetType.None:
                    SwitchToCustomized(restriction, true);
                    ResoniteMod.Msg($"Preset Customized changed {restriction.Name} to {boolValue}.");
                    return;
                case PresetType.All when boolValue:
                    return;
                case PresetType.All:
                    SwitchToCustomized(restriction, false);
                    ResoniteMod.Msg($"Preset Customized changed {restriction.Name} to {boolValue}.");
                    return;
                case PresetType.Customized:
                case PresetType.StoredPresetAlpha:
                case PresetType.StoredPresetBeta:
                case PresetType.StoredPresetGamma:
                case PresetType.StoredPresetDelta:
                case PresetType.StoredPresetOmega:
                default:
                    var customStored = GetCustomStored(presetType);
                    if (customStored[restriction.Index] == boolValue) return;
                    customStored.Set(restriction.Index, boolValue);
                    SetCustomStored(presetType, customStored);
                    ResoniteMod.Msg($"Preset {presetType} changed {restriction.Name} to {boolValue}.");
                    return;
            }
        };
    }

    private void SwitchToCustomized(IRestriction restriction, bool value)
    {
        var customStored = GetCustomStored(PresetType.Customized);
        customStored.SetAll(!value);
        customStored.Set(restriction.Index, value);
        SetCustomStored(PresetType.Customized, customStored);
        _config?.Set(_presetConfig, PresetType.Customized);
    }

    private BitArray GetCustomStored(PresetType presetType)
    {
        if (_config == null) return new BitArray(Restrictions.Length, false);
        if (presetType == PresetType.None) return new BitArray(Restrictions.Length, false);
        if (presetType == PresetType.All) return new BitArray(Restrictions.Length, true);
        var savedPresetFound = _config.TryGetValue(_presetStore[presetType], out var value);
        if (!savedPresetFound || value == null) return new BitArray(Restrictions.Length, false);
        var bitArray = new BitArray(Restrictions.Length, false);
        foreach (var entry in value)
        {
            if (entry.Key == null) continue;
            var found = Restrictions.TryGetByName(entry.Key, out var restriction);
            if (!found || restriction == null) continue;
            bitArray.Set(restriction.Index, entry.Value);
        }

        return bitArray;
    }

    private void SetCustomStored(PresetType presetType, BitArray bitArray)
    {
        var dictionary = Restrictions.All.ToDictionary(restriction => restriction.Name,
            restriction => bitArray[restriction.Index]);
        _config?.Set(_presetStore[presetType], dictionary);
    }

    private void OnPresetSelected(object? value)
    {
        ResoniteMod.Msg($"Restrainite preset changed to {value}.");
        var selectedPreset = value as PresetType? ?? PresetType.None;
        var restrictionValues = GetCustomStored(selectedPreset);
        foreach (var restriction in Restrictions.All)
        {
            var configurationKey = GetDisplayedRestrictionConfig(restriction);
            var restrictionValue = restrictionValues[restriction.Index];
            if (_config?.GetValue(configurationKey) == restrictionValue) continue;
            _config?.Set(configurationKey, restrictionValue);
        }

        var passwordPreset = selectedPreset is PresetType.All or PresetType.None
            ? PresetType.Customized
            : selectedPreset;
        if ((_config?.TryGetValue(_presetPasswords, out var presetPasswords) ?? false) &&
            (presetPasswords?.TryGetValue(passwordPreset.ToString(), out var password) ?? false))
            _config?.Set(_password, password ?? string.Empty);
        else
            _config?.Set(_password, string.Empty);

        if (selectedPreset == PresetType.All && (!_config?.GetValue(_hasSeenAllPresetWarning) ?? false))
        {
            Userspace.UserspaceWorld.DisplayNotice(
                "Restrainite Warning",
                "You have activated the 'All' preset in Restrainite. Using this preset is <u>discouraged" +
                "</u>, unless you are familiar with all the ways you could end up in an " +
                "undesirable, restricted state.\n\nPlease read the documentation on <i>restrainite.github.io</i>. " +
                "Consider using <b>stored presets</b> and only allowing " +
                "restriction types you are comfortable with.\n<i>This is the only time you will see this message.</i>",
                OfficialAssets.Graphics.Icons.General.ExclamationPoint);
            _config?.Set(_hasSeenAllPresetWarning, true);
        }
    }

    private ModConfigurationKey<bool> GetDisplayedRestrictionConfig(IRestriction restriction)
    {
        return _displayedRestrictions[restriction.Index];
    }

    internal bool IsRestrictionEnabled(IRestriction restriction)
    {
        var key = GetDisplayedRestrictionConfig(restriction);
        var configValue = false;
        var foundConfigValue = _config?.TryGetValue(key, out configValue) ?? false;
        return foundConfigValue && configValue;
    }

    internal void OnWorldPermissionChanged(World world)
    {
        // If not in the focused world, we only trigger recheck permissions, which will remove it from those worlds
        // which should be restricted.
        if (world != world.WorldManager.FocusedWorld)
        {
            ShouldRecheckPermissions.SafeInvoke();
            return;
        }

        // Focused world, we change the preset.
        var currentPreset = _config?.GetValue(_presetConfig);
        var changePreset = GetWorldPresetChangeType(world);
        TriggerPresetUpdate(changePreset, currentPreset);
    }

    private void TriggerPresetUpdate(PresetChangeType? changePreset, PresetType? currentPreset)
    {
        switch (changePreset)
        {
            case PresetChangeType.None:
                if (currentPreset == PresetType.None)
                {
                    ShouldRecheckPermissions.SafeInvoke();
                    return;
                }

                _config?.Set(_presetConfig, PresetType.None);
                return;
            case PresetChangeType.All:
            case PresetChangeType.StoredPresetAlpha:
            case PresetChangeType.StoredPresetBeta:
            case PresetChangeType.StoredPresetGamma:
            case PresetChangeType.StoredPresetDelta:
            case PresetChangeType.StoredPresetOmega:
                if (currentPreset == (PresetType)changePreset)
                {
                    ShouldRecheckPermissions.SafeInvoke();
                    return;
                }

                _config?.Set(_presetConfig, (PresetType)changePreset);
                return;
            case PresetChangeType.DoNotChange:
            case null:
                ShouldRecheckPermissions.SafeInvoke();
                return;
            default:
                throw new ArgumentOutOfRangeException(nameof(changePreset), changePreset, "Unknown preset change type");
        }
    }

    private PresetChangeType? GetWorldPresetChangeType(World? world)
    {
        var worldPermissionType = world?.ToWorldPermissionType();
        if (worldPermissionType == null) return null;
        var found = _changeOnWorldPermissionChangeDict.TryGetValue(
            (WorldPermissionType)worldPermissionType, out var key);
        return !found || key == null ? null! : _config?.GetValue(key);
    }

    internal bool AllowRestrictionsFromWorld(World? world, IRestriction? restriction = null)
    {
        if (_config?.GetValue(_presetConfig) == PresetType.None) return false;

        if (restriction != null && !IsRestrictionEnabled(restriction)) return false;

        if (IsLocalHome(world) || IsLocalHome(world?.WorldManager.FocusedWorld)) return false;

        return world == world?.WorldManager.FocusedWorld ||
               world == Userspace.UserspaceWorld ||
               (
                   !(_config?.GetValue(_allowRestrictionsFromFocusedWorldOnly) ?? true) &&
                   GetWorldPresetChangeType(world) != PresetChangeType.None
               );
    }

    private static bool IsLocalHome(World? world)
    {
        return IdUtil.GetOwnerType(world?.CorrespondingRecord?.OwnerId) == OwnerType.Machine;
    }

    internal bool IsCorrectPassword(string? passwordToCheck)
    {
        // If no password is set, we accept any password
        if (!RequiresPassword ||
            !(_config?.TryGetValue(_password, out var password) ?? false) ||
            password == null)
            return true;

        // Password is set, so compare it
        return passwordToCheck != null && password.Equals(passwordToCheck, StringComparison.Ordinal);
    }

    private void UpdatePasswordState(object? value)
    {
        var password = value as string ?? string.Empty;
        RequiresPassword = !string.IsNullOrEmpty(password);

        var presetType = _config?.GetValue(_presetConfig) ?? PresetType.Customized;
        presetType = presetType is PresetType.All or PresetType.None ? PresetType.Customized : presetType;

        if (!(_config?.TryGetValue(_presetPasswords, out var presetPasswords) ?? false)
            || presetPasswords == null)
            presetPasswords = new Dictionary<string, string>();
        presetPasswords[presetType.ToString()] = password;
        _config?.Set(_presetPasswords, presetPasswords);

        ShouldRecheckPermissions.SafeInvoke();
    }
}