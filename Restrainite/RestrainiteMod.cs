using System;
using System.Collections.Immutable;
using FrooxEngine;
using HarmonyLib;
using ResoniteModLoader;
using Restrainite.Enums;
using Restrainite.Patches;

namespace Restrainite;

public class RestrainiteMod : ResoniteMod
{
    public const string LogReportUrl =
        "Please report this to Restrainite (https://github.com/Restrainite/Restrainite/issues):";

    internal static readonly Configuration Configuration = new();

    public override string Name => "Restrainite";
    public override string Author => "SnepDrone Zenuru";

    public static Version AssemblyVersion => typeof(RestrainiteMod).Assembly.GetName().Version;
    public override string Version => $"{AssemblyVersion.Major}.{AssemblyVersion.Minor}.{AssemblyVersion.Build}";

    public override string Link => "https://restrainite.github.io";

    internal static bool SuccessfullyPatched { get; private set; } = true;

    /**
     * OnRestrictionChanged will fire, when the restriction is activated or deactivated. It will take into account, if
     * the restriction is disabled by the user. It will run in the update cycle of the world that triggered the
     * change. This doesn't have to be the focused world, so make sure, that any write operation are run in the next
     * update cycle. The value is debounced, meaning it will only trigger, if it actually changes.
     */
    internal static event Action<PreventionType, bool>? OnRestrictionChanged;

    /**
     * OnFloatChanged will fire, when the float value is changed. It will take into account, if
     * the restriction is disabled by the user. It will run in the update cycle of the world that triggered the
     * change. This doesn't have to be the focused world, so make sure, that any write operation are run in the next
     * update cycle. The value is debounced, meaning it will only trigger, if it actually changes.
     */
    internal static event Action<PreventionType, float>? OnFloatChanged;

    /**
     * OnStringSetChanged will fire, when the string set value is changed. It will take into account, if
     * the restriction is disabled by the user. It will run in the update cycle of the world that triggered the
     * change. This doesn't have to be the focused world, so make sure, that any write operation are run in the next
     * update cycle. The value is debounced, meaning it will only trigger, if it actually changes.
     */
    internal static event Action<PreventionType, IImmutableSet<string>>? OnStringSetChanged;

    public override void DefineConfiguration(ModConfigurationDefinitionBuilder builder)
    {
        Configuration.DefineConfiguration(builder);
    }

    /*
     * There are more graceful ways to handle incompatible configs, but this is the simplest.
     * Default is ERROR (prevents saving), CLOBBER overwrites the config file.
     */
    public override IncompatibleConfigurationHandlingOption HandleIncompatibleConfigurationVersions(
        Version serializedVersion, Version definedVersion)
    {
        return IncompatibleConfigurationHandlingOption.CLOBBER;
    }

    public override void OnEngineInit()
    {
        Configuration.Init(GetConfiguration());

        PatchResonite();

        InitializePatches();
    }

    private static void PatchResonite()
    {
        var harmony = new Harmony("Restrainite.Restrainite");

        AccessTools.GetTypesFromAssembly(typeof(RestrainiteMod).Assembly)
            .Do<Type>(type =>
            {
                try
                {
                    harmony.CreateClassProcessor(type).Patch();
                }
                catch (Exception ex)
                {
                    Error($"{LogReportUrl} Failed to patch {type.FullName}: {ex}");
                    SuccessfullyPatched = false;
                }
            });
    }

    private static void InitializePatches()
    {
        EnforceWhispering.Initialize();
        PreventGrabbing.Initialize();
        PreventOpeningContextMenu.Initialize();
        PreventOpeningDash.Initialize();
        PreventLaserTouch.Initialize();
        PreventUserScaling.Initialize();
        ShowOrHideUserAvatars.Initialize();
        DisableNameplates.Initialize();
        ShowOrHideDashScreens.Initialize();
        PreventHearing.Initialize();
        MaximumHearingDistance.Initialize();
        PreventReading.Initialize();
        TrackerMovementSpeed.Initialize();
        SetBusyStatus.Initialize();
        PreventEditMode.Initialize();
    }

    internal static bool IsRestricted(PreventionType preventionType)
    {
        return DynamicVariableSpaceSync.GetGlobalState(preventionType);
    }

    internal static IImmutableSet<string> GetStringSet(PreventionType preventionType)
    {
        return DynamicVariableSpaceSync.GetGlobalStringSet(preventionType);
    }

    internal static string StringSetAsString(IImmutableSet<string> set)
    {
        return set.Join(t => t, ",");
    }

    /**
     * Only to be called by DynamicVariableSpaceSync.
     */
    internal static void NotifyRestrictionChanged(World source, PreventionType preventionType, bool value)
    {
        source.RunInUpdates(0, () => OnRestrictionChanged.SafeInvoke(preventionType, value));
    }

    /**
     * Only to be called by DynamicVariableSpaceSync.
     */
    internal static void NotifyFloatChanged(World source, PreventionType preventionType, float value)
    {
        source.RunInUpdates(0, () => OnFloatChanged.SafeInvoke(preventionType, value));
    }

    internal static float GetLowestFloat(PreventionType preventionType)
    {
        return DynamicVariableSpaceSync.GetLowestGlobalFloat(preventionType);
    }

    /**
     * Only to be called by DynamicVariableSpaceSync.
     */
    internal static void NotifyStringSetChanged(World source, PreventionType preventionType,
        IImmutableSet<string> value)
    {
        source.RunInUpdates(0, () => OnStringSetChanged.SafeInvoke(preventionType, value));
    }
}