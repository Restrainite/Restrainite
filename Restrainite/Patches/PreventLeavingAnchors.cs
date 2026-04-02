using FrooxEngine;
using FrooxEngine.CommonAvatar;
using HarmonyLib;
using Restrainite.RestrictionTypes.Base;

namespace Restrainite.Patches;

[HarmonyPatch]
internal static class PreventLeavingAnchors
{
    private static readonly ThreadLocal<bool> IsAnchoring = new();

    private static bool _disabled;

    public static void Initialize()
    {
        Restrictions.PreventLeavingAnchors.OnChanged += OnChanged;
        UserRootInjector.OnUserRootInitialized += OnUserRootInitialized;
    }

    private static void OnUserRootInitialized(UserRoot userRoot)
    {
        if (userRoot.World != Engine.Current.WorldManager.FocusedWorld) return;
        // Delay to allow the user to be fully initialized.
        userRoot.RunInUpdates(1, () => { WaitForUserToStopFalling(userRoot); });
    }

    private static void WaitForUserToStopFalling(UserRoot userRoot)
    {
        // Make sure the user is on the ground, otherwise they will be stuck in fall animation.
        if (userRoot.GetRegisteredComponent<LocomotionController>()?.ActiveModule is IPhysicalLocomotion activeModule)
        {
            var characterController = activeModule.CharacterController;
            if (characterController?.CurrentGround != null)
                userRoot.RunInUpdates(1, () => { OnChanged(Restrictions.PreventLeavingAnchors); });
            else
                userRoot.RunInUpdates(1, () => { WaitForUserToStopFalling(userRoot); });
        }
        else
        {
            OnChanged(Restrictions.PreventLeavingAnchors);
        }
    }

    private static void OnChanged(IRestriction restriction)
    {
        if (!Restrictions.PreventLeavingAnchors.IsRestricted)
        {
            _disabled = false;
            return;
        }

        var world = Engine.Current?.WorldManager?.FocusedWorld;
        if (world == null) return;
        var anchor = Restrictions.PreventLeavingAnchors.AvatarAnchors.GetRandomAnchor(world);
        if (anchor == null)
        {
            _disabled = false;
            return;
        }

        if (_disabled) return;
        var localUser = world.LocalUser;
        localUser?.Root?.RunSynchronously(() =>
        {
            if (localUser.IsAnchored())
                if (localUser.GetCurrentAnchor() is not AvatarAnchor currentAnchor ||
                    Restrictions.PreventLeavingAnchors.AvatarAnchors.Contains(currentAnchor))
                    return;
            anchor.Anchor(localUser);
            if (!localUser.IsAnchored()) _disabled = true;
        });
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(AvatarAnchor), nameof(AvatarAnchor.Release))]
    private static bool AvatarAnchor_Release_Prefix(AvatarAnchor __instance)
    {
        return __instance.Engine.WorldManager.FocusedWorld.LocalUser != __instance.AnchoredUser
               || IsAnchoring.Value || !Restrictions.PreventLeavingAnchors.IsRestricted;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(AvatarAnchor), nameof(AvatarAnchor.Anchor), typeof(AvatarManager))]
    private static bool AvatarAnchor_Anchor_Prefix(AvatarAnchor __instance)
    {
        if (Restrictions.PreventLeavingAnchors.IsRestricted &&
            !Restrictions.PreventLeavingAnchors.AvatarAnchors.Contains(__instance)) return false;
        IsAnchoring.Value = true;
        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(AvatarAnchor), nameof(AvatarAnchor.Anchor), typeof(AvatarManager))]
    private static void AvatarAnchor_Anchor_Postfix()
    {
        IsAnchoring.Value = false;
    }
}