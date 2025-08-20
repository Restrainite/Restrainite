using System;
using FrooxEngine;
using HarmonyLib;
using Renderite.Shared;
using ResoniteModLoader;

namespace Restrainite.Patches;

[HarmonyPatch]
internal static class PreventEmergencyRespawning
{
    private static WeakReference<World>? _panicTriggeredForWorld;

    [HarmonyPrefix]
    [HarmonyPatch(typeof(InteractionHandler), "HoldMenu")]
    private static void InteractionHandler_HoldMenu_Prefix(ref float ___panicCharge,
        InteractionHandler __instance)
    {
        if (Restrictions.PreventEmergencyRespawning.IsRestricted)
        {
            ___panicCharge = -__instance.Time.Delta;
        }
        else if (__instance.World == Userspace.UserspaceWorld &&
                 __instance.IsNearHead &&
                 __instance.OtherTool != null &&
                 __instance.Side.Value == Chirality.Left &&
                 __instance.OtherTool.Inputs.Menu.Held &&
                 __instance.OtherTool.IsNearHead &&
                 ___panicCharge + __instance.Time.Delta >= 2.0 &&
                 (__instance.Inputs.Grab.Held || __instance.OtherTool.Inputs.Grab.Held))
        {
            ResoniteMod.Msg("Detected Emergency Respawn");
            var focusedWorld = __instance.World.Engine?.WorldManager?.FocusedWorld;
            if (focusedWorld == null) return;
            _panicTriggeredForWorld = new WeakReference<World>(focusedWorld);
            __instance.World.RunInUpdates(2, () =>
            {
                ResoniteMod.Msg("Resetting Emergency Respawn");
                _panicTriggeredForWorld = null;
            });
        }
    }

    internal static bool IsEmergencyRespawning(World? world)
    {
        return world != null &&
               _panicTriggeredForWorld != null &&
               _panicTriggeredForWorld.TryGetTarget(out var worldTarget) &&
               world == worldTarget;
    }
}