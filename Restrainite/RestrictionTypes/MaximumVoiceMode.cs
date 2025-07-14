using Restrainite.RestrictionTypes.Base;

namespace Restrainite.RestrictionTypes;

internal class MaximumVoiceMode : BaseRestriction
{
    public override string Name => "Maximum Voice Mode";
    public override string Description => "Should others be able to modify your voice mode?";

    public LowestVoiceModeParameter LowestVoiceMode { get; } = new();

    protected override IRestrictionParameter[] InitRestrictionParameters()
    {
        return [LowestVoiceMode];
    }
}