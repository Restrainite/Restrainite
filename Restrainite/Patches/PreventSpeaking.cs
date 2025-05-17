using Elements.Core;
using FrooxEngine;
using HarmonyLib;
using Restrainite.Enums;

namespace Restrainite.Patches;

[HarmonyPatch]
internal static class PreventSpeaking
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(AudioSystem), nameof(AudioSystem.IsMuted), MethodType.Getter)]
    private static void PreventSpeaking_AudioSystemIsMuted_Getter_Postfix(ref bool __result)
    {
        if (RestrainiteMod.IsRestricted(PreventionType.PreventSpeaking))
        {
            __result = true;
            return;
        }

        if (RestrainiteMod.IsRestricted(PreventionType.SpeakingVolume))
        {
            var multiplier = RestrainiteMod.GetLowestFloat(PreventionType.SpeakingVolume);
            if (float.IsNaN(multiplier)) return;
            if (MathX.Approximately(multiplier, 0.0f)) __result = true;
        }
    }
}