using Elements.Core;
using FrooxEngine;
using HarmonyLib;

namespace Restrainite.Patches;

[HarmonyPatch]
internal static class PreventSpeaking
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(AudioSystem), nameof(AudioSystem.IsMuted), MethodType.Getter)]
    private static void AudioSystem_IsMuted_Getter_Postfix(ref bool __result)
    {
        if (Restrictions.PreventSpeaking.IsRestricted)
        {
            __result = true;
            return;
        }

        if (Restrictions.SpeakingVolume.IsRestricted)
        {
            var multiplier = Restrictions.SpeakingVolume.LowestFloat.Value;
            if (float.IsNaN(multiplier)) return;
            if (MathX.Approximately(multiplier, 0.0f)) __result = true;
        }
    }
}