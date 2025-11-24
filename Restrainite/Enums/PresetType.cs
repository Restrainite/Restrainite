namespace Restrainite.Enums;

internal enum PresetType
{
    None,
    Customized,
    StoredPresetAlpha,
    StoredPresetBeta,
    StoredPresetGamma,
    StoredPresetDelta,
    StoredPresetOmega,
    All
}

internal static class PresetTypes
{
    internal static readonly IEnumerable<PresetType> List =
        Enum.GetValues<PresetType>();

    internal static readonly int Max = (int)List.Max() + 1;
}