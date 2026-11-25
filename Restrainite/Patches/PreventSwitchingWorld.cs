using System.Reflection;
using FrooxEngine;
using HarmonyLib;
using ProtoFlux.Runtimes.Execution.Nodes.FrooxEngine.Worlds;

namespace Restrainite.Patches;

// Do not prevent all calls to FrooxEngine.WorldManager.FocusWorld, FrooxEngine.Userspace.OpenWorld and
// FrooxEngine.Userspace.ExitWorld, because it will break things like bans.

internal static class PreventSwitchingWorld
{
    private static readonly ThreadLocal<bool> InOnCommonUpdate = new();
    private static readonly ThreadLocal<bool> InInteractionHandlerHoldMenu = new();

    [HarmonyPatch]
    private static class CloseWorldPatches
    {
        private static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(LegacyWorldThumbnailItem), "OnClose");
            yield return AccessTools.Method(typeof(WorldCloseAction), "Pressed");
            yield return AccessTools.Method(typeof(WorldSwitcher), "WorldOrbLongPressed");
        }

        private static bool Prefix()
        {
            return !Restrictions.PreventSwitchingWorld.IsRestricted;
        }
    }

    [HarmonyPatch]
    private static class UserspaceExitWorldPatches
    {
        private static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(Userspace), "ExitWorld");
        }

        private static bool Prefix(ref Task __result)
        {
            if ((!InOnCommonUpdate.Value && !InInteractionHandlerHoldMenu.Value) ||
                !Restrictions.PreventSwitchingWorld.IsRestricted) return true;
            __result = Task.CompletedTask;
            return false;
        }
    }


    [HarmonyPatch]
    private static class OpenWorldPatches
    {
        private static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(LegacyWorldDetail), "OnOpen");
            yield return AccessTools.Method(typeof(WorldSwitcher), "WorldOrbTouched");
            yield return AccessTools.Method(typeof(NewWorldDialog), "OnStartSession");
        }

        private static bool Prefix()
        {
            return !Restrictions.PreventSwitchingWorld.IsRestricted;
        }
    }


    [HarmonyPatch]
    private static class UserspaceOpenWorldPatches
    {
        private static IEnumerable<MethodBase> TargetMethods()
        {
            // These methods all call Userspace.OpenWorld
            yield return AccessTools.Method(typeof(WorldOrb), "Open");
            yield return AccessTools.Method(typeof(WorldOrb), "OpenLink");
            yield return AccessTools.Method(typeof(LegacyWorldDetail), "CustomWorldStart");
            yield return AccessTools.Method(typeof(LegacyWorldDetail), "OnOpen");
            yield return AccessTools.Method(typeof(WorldOrb), "CustomWorldStart");
            yield return AccessTools.Method(typeof(ContactItem), "OnJoin");
            yield return AccessTools.Method(typeof(SessionItem), "OnJoin");
            yield return AccessTools.Method(typeof(InventoryBrowser), "OnOpenWorld");
            yield return AccessTools.Method(typeof(SessionInfoSource), "OpenLink");
            yield return AccessTools.Method(typeof(WorldLink), "OpenLink");
            yield return AccessTools.Method(typeof(ButtonOpenHome), "Pressed");
        }

        private static bool Prefix()
        {
            return !Restrictions.PreventSwitchingWorld.IsRestricted;
        }
    }

    // Prevent world switching via the FocusWorld and OpenWorld ProtoFlux nodes
    [HarmonyPatch]
    private static class WorldActionNodePatches
    {
        private static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(FocusWorld), "IsAllowedToFocus");
            yield return AccessTools.Method(typeof(WorldStartSettings), "CanStart");
        }

        private static bool Prefix(ref bool __result)
        {
            if (!Restrictions.PreventSwitchingWorld.IsRestricted) return true;
            __result = false;
            return false;
        }
    }

    [HarmonyPatch]
    private static class SteamJoinPatch
    {
        [HarmonyPatch(typeof(SteamConnector), "GameRichPresenceJoinRequested")]
        [HarmonyPrefix]
        private static bool GameRichPresenceJoinRequested_Prefix()
        {
            return !Restrictions.PreventSwitchingWorld.IsRestricted;
        }
    }

    // Prevent world switching via LCtrl + Tab, or closing of the world via LCtrl + LShift + Q or LShift + Esc
    [HarmonyPatch]
    private static class UserspaceOnCommonUpdatePatch
    {
        [HarmonyPatch(typeof(Userspace), "OnCommonUpdate")]
        [HarmonyPrefix]
        private static void OnCommonUpdate_Prefix()
        {
            InOnCommonUpdate.Value = true;
        }

        [HarmonyPatch(typeof(Userspace), "OnCommonUpdate")]
        [HarmonyFinalizer]
        private static void OnCommonUpdate_Finalizer()
        {
            InOnCommonUpdate.Value = false;
        }
    }

    // Prevent world closing via emergency gesture
    [HarmonyPatch]
    private static class InteractionHandlerHoldMenuPatch
    {
        [HarmonyPatch(typeof(InteractionHandler), "HoldMenu")]
        [HarmonyPrefix]
        private static void HoldMenu_Prefix()
        {
            InInteractionHandlerHoldMenu.Value = true;
        }

        [HarmonyPatch(typeof(InteractionHandler), "HoldMenu")]
        [HarmonyFinalizer]
        private static void HoldMenu_Finalizer()
        {
            InInteractionHandlerHoldMenu.Value = false;
        }
    }

    [HarmonyPatch]
    private static class WorldManagerPatch
    {
        [HarmonyPatch(typeof(WorldManager), nameof(WorldManager.FocusWorld))]
        [HarmonyPrefix]
        private static bool Prefix()
        {
            return !(InOnCommonUpdate.Value && Restrictions.PreventSwitchingWorld.IsRestricted);
        }
    }
}