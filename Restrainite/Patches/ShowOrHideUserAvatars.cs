using System.Diagnostics;
using System.Reflection;
using Elements.Core;
using FrooxEngine;
using FrooxEngine.CommonAvatar;
using FrooxEngine.PhotonDust;
using FrooxEngine.UIX;
using HarmonyLib;
using ResoniteModLoader;
using Restrainite.RestrictionTypes.Base;

namespace Restrainite.Patches;

[HarmonyPatch]
internal static class ShowOrHideUserAvatars
{
    private static readonly SlowUpdater Updater = new();

    internal static void Initialize()
    {
        Restrictions.ShowUserAvatars.OnChanged += OnRestrictionChanged;
        Restrictions.HideUserAvatars.OnChanged += OnRestrictionChanged;
    }

    private static void OnRestrictionChanged(IRestriction restriction)
    {
        Updater.Start();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(AvatarNameplateVisibilityDriver), nameof(AvatarNameplateVisibilityDriver.ShouldBeVisible),
        MethodType.Getter)]
    private static void AvatarNameplateVisibilityDriver_ShouldBeVisible_Postfix(ref bool __result,
        AvatarNameplateVisibilityDriver __instance)
    {
        ShouldBeVisible(ref __result, __instance);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(AvatarLiveIndicator), nameof(AvatarLiveIndicator.ShouldBeVisible), MethodType.Getter)]
    private static void AvatarLiveIndicator_ShouldBeVisible_Postfix(ref bool __result, AvatarLiveIndicator __instance)
    {
        ShouldBeVisible(ref __result, __instance);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GaussianSplatRenderer), nameof(GaussianSplatRenderer.ShouldBeEnabled), MethodType.Getter)]
    private static void GaussianSplatRenderer_ShouldBeEnabled_Postfix(ref bool __result,
        GaussianSplatRenderer __instance)
    {
        ShouldBeVisible(ref __result, __instance);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Light), nameof(Light.ShouldBeEnabled), MethodType.Getter)]
    private static void Light_ShouldBeEnabled_Postfix(ref bool __result, Light __instance)
    {
        ShouldBeVisible(ref __result, __instance);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(MeshRenderer), nameof(MeshRenderer.ShouldBeEnabled), MethodType.Getter)]
    private static void MeshRenderer_ShouldBeEnabled_Postfix(ref bool __result, MeshRenderer __instance)
    {
        ShouldBeVisible(ref __result, __instance);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(TextRenderer), nameof(TextRenderer.ShouldBeEnabled), MethodType.Getter)]
    private static void TextRenderer_ShouldBeEnabled_Postfix(ref bool __result, TextRenderer __instance)
    {
        ShouldBeVisible(ref __result, __instance);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Canvas), nameof(Canvas.ShouldBeEnabled), MethodType.Getter)]
    private static void Canvas_ShouldBeEnabled_Postfix(ref bool __result, Canvas __instance)
    {
        ShouldBeVisible(ref __result, __instance);
    }

    private static void ShouldBeVisible(ref bool result, Component component)
    {
        if (!result ||
            (!Restrictions.ShowUserAvatars.IsRestricted && !Restrictions.HideUserAvatars.IsRestricted) ||
            component.Slot?.IsLocalElement != false) return;
        var user = component.Slot?.ActiveUser;
        if (user == null || user.IsLocalUser) return;
        if (Restrictions.ShowUserAvatars.IsRestricted &&
            !Restrictions.ShowUserAvatars.StringSet.Contains(user.UserID))
        {
            result = false;
            return;
        }

        if (!Restrictions.HideUserAvatars.IsRestricted ||
            !Restrictions.HideUserAvatars.StringSet.Contains(user.UserID)) return;
        result = false;
    }

    [HarmonyPatch]
    private static class ParticleSystemPatch
    {
        private static IEnumerable<MethodInfo> TargetMethods()
        {
            return AccessTools.GetDeclaredMethods(typeof(ParticleSystem))
                .Where(method => method.Name.Contains("ShouldBeEnabled")).AsEnumerable();
        }

        private static void Postfix(ref bool __result, ParticleSystem __instance)
        {
            ShouldBeVisible(ref __result, __instance);
        }
    }

    /// <summary>
    ///     To avoid lagging the game, we update the visibility of all avatars for each user in small chunks. Every update
    ///     cycle, we mark one component in one user dirty.
    /// </summary>
    private sealed class SlowUpdater
    {
        private const int TotalComponentCount = 8;
        private readonly List<RefID> _finishedUsers = [];
        private string? _currentSessionId;
        private RefID? _currentUserId;
        private int _finishedTypesForCurrentUser;
        private ImmutableStringSet? _hiddenUsers;
        private bool _hiddenUsersState;
        private int _isRunning;
        private ImmutableStringSet? _shownUsers;
        private bool _shownUsersState;

        public void Start()
        {
            var world = Engine.Current?.WorldManager?.FocusedWorld;
            if (world == null) return;
            if (Interlocked.Exchange(ref _isRunning, 1) == 1) return;
            world.RunInUpdates(1, Update);
        }

        private void Update()
        {
            try
            {
                var world = Engine.Current?.WorldManager?.FocusedWorld;
                if (world == null)
                {
                    Finish();
                    return;
                }

                ResetStateIfNecessary(world);

                var stopwatch = Stopwatch.StartNew();

                do
                {
                    var currentUserSlot = CurrentUserSlot(world, _currentUserId) ?? NextUserSlot(world);

                    if (currentUserSlot == null || _currentUserId == null)
                    {
                        Finish();
                        return;
                    }
                
                    MarkChangeDirty(_finishedTypesForCurrentUser, currentUserSlot);
                    _finishedTypesForCurrentUser++;

                    if (_finishedTypesForCurrentUser >= TotalComponentCount)
                    {
                        _finishedUsers.Add((RefID)_currentUserId);
                        _currentUserId = null;
                        _finishedTypesForCurrentUser = 0;
                    }
                } while (stopwatch.Elapsed.TotalMilliseconds < 1);

                world.RunInUpdates(1, Update);
            }
            catch (Exception e)
            {
                ResoniteMod.Warn("Error while updating user avatars", e);
                Finish();
            }
        }

        private static Slot? CurrentUserSlot(World world, RefID? userId)
        {
            if (userId == null) return null;
            var user = world.GetUser(userId.Value);
            var currentUserSlot = user?.Root?.Slot;
            return currentUserSlot;
        }

        private Slot? NextUserSlot(World world)
        {
            var user = world.TryGetUser(user => !_finishedUsers.Contains(user.ReferenceID) && user is { IsLocalUser: false, Root: not null, Root.Slot: not null });
            _currentUserId = user?.ReferenceID;
            _finishedTypesForCurrentUser = 0;
            return user?.Root?.Slot;
        }

        private void Finish()
        {
            Reset();
            Interlocked.Exchange(ref _isRunning, 0);
        }

        private void ResetStateIfNecessary(World world)
        {
            if (_shownUsersState == Restrictions.ShowUserAvatars.IsRestricted &&
                _hiddenUsersState == Restrictions.HideUserAvatars.IsRestricted &&
                _shownUsers != null && _shownUsers.Equals(Restrictions.ShowUserAvatars.StringSet.Value) &&
                _hiddenUsers != null && _hiddenUsers.Equals(Restrictions.HideUserAvatars.StringSet.Value) &&
                _currentSessionId == world.SessionId) return;
            Reset(world.SessionId);
        }

        private void Reset(string? sessionId = null)
        {
            _shownUsers = Restrictions.ShowUserAvatars.StringSet.Value;
            _hiddenUsers = Restrictions.HideUserAvatars.StringSet.Value;
            _shownUsersState = Restrictions.ShowUserAvatars.IsRestricted;
            _hiddenUsersState = Restrictions.HideUserAvatars.IsRestricted;
            _finishedUsers.Clear();
            _currentUserId = null;
            _finishedTypesForCurrentUser = 0;
            _currentSessionId = sessionId;
        }

        /// <summary>
        ///     This list of components has been created by searching for all components that are using the
        ///     <see cref="User.IsRenderingLocallyBlocked" /> property.
        /// </summary>
        /// <param name="component">Internal id of the component</param>
        /// <param name="slot">User Root Slot</param>
        private static void MarkChangeDirty(int component, Slot slot)
        {
            switch (component)
            {
                case 0:
                    slot.ForeachComponentInChildren<AvatarNameplateVisibilityDriver>(c => c?.MarkChangeDirty());
                    break;
                case 1:
                    slot.ForeachComponentInChildren<AvatarLiveIndicator>(c => c?.MarkChangeDirty());
                    break;
                case 2:
                    slot.ForeachComponentInChildren<GaussianSplatRenderer>(c => c?.MarkChangeDirty());
                    break;
                case 3:
                    slot.ForeachComponentInChildren<Light>(c => c?.MarkChangeDirty());
                    break;
                case 4:
                    slot.ForeachComponentInChildren<MeshRenderer>(c => c?.MarkChangeDirty());
                    break;
                case 5:
                    slot.ForeachComponentInChildren<ParticleSystem>(c => c?.MarkChangeDirty());
                    break;
                case 6:
                    slot.ForeachComponentInChildren<TextRenderer>(c => c?.MarkChangeDirty());
                    break;
                case 7:
                    slot.ForeachComponentInChildren<Canvas>(c => c?.MarkChangeDirty());
                    break;
                // Remember to update TotalComponentCount when adding new components
            }
        }
    }
}