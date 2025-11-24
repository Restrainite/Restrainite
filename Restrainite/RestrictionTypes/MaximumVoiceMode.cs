using Restrainite.RestrictionTypes.Base;

namespace Restrainite.RestrictionTypes;

internal sealed class MaximumVoiceMode : BaseRestriction
{
    public override string Name => "Maximum Voice Mode";

    public override string Description =>
        "Should others be able to limit your voice mode? Prevent Shouting or enforce whispering?";

    public LowestVoiceModeParameter LowestVoiceMode { get; } = new();

    protected override IRestrictionParameter[] InitRestrictionParameters()
    {
        return [LowestVoiceMode];
    }
}