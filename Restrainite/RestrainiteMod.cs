using System;
using HarmonyLib;
using ResoniteModLoader;
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
}