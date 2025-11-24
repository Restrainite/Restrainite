using FrooxEngine;

namespace Restrainite.RestrictionTypes.Base;

internal sealed class LowestVoiceModeParameter : IRestrictionParameter
{
    private SimpleState<VoiceMode> LowestVoiceMode { get; } = new(VoiceMode.Broadcast);

    public VoiceMode Value => LowestVoiceMode.Value;

    public bool Combine(IRestriction restriction, IEnumerable<IBaseState> states)
    {
        var lowestVoiceMode = VoiceMode.Broadcast;
        foreach (var baseState in states)
        {
            if (baseState is not LocalBaseState<VoiceMode> localState) continue;
            if (lowestVoiceMode > localState.Value) lowestVoiceMode = localState.Value;
        }

        return LowestVoiceMode.SetIfChanged(restriction, lowestVoiceMode);
    }

    public IBaseState CreateLocalState(DynamicVariableSpace dynamicVariableSpace,
        IDynamicVariableSpace dynamicVariableSpaceSync,
        IRestriction restriction)
    {
        return new LocalBaseState<VoiceMode>(VoiceMode.Broadcast, dynamicVariableSpace, dynamicVariableSpaceSync,
            restriction, false);
    }

    public void CreateStatusComponent(IRestriction restriction, Slot slot, string dynamicVariableSpaceName)
    {
        BaseRestriction.CreateStatusComponent(restriction, slot, dynamicVariableSpaceName, LowestVoiceMode, a => a);
    }
}