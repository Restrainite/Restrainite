using System.Collections.Generic;
using Elements.Core;
using FrooxEngine;
using HarmonyLib;
using Restrainite.Enums;

namespace Restrainite.Patches;

[HarmonyPatch]
internal static class TrackerMovementSpeed
{
    private static readonly Dictionary<BodyNode, SmoothingFilter> SmoothingFilters = new();

    internal static void Initialize()
    {
        RestrainiteMod.OnRestrictionChanged += OnChange;
    }

    private static void OnChange(PreventionType preventionType, bool value)
    {
        if (preventionType == PreventionType.TrackerMovementSpeed && !value) SmoothingFilters.Clear();
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(UserPoseController), nameof(UserPoseController.FilterDeviceNode))]
    private static void UserPoseController_FilterDeviceNode_Prefix(
        UserPoseController __instance,
        ref float3 position,
        ref floatQ rotation,
        ref bool isActive,
        BodyNode node)
    {
        if (RestrainiteMod.IsRestricted(PreventionType.DisableVRTrackers))
        {
            isActive = false;
            return;
        }
        if (!__instance.IsUnderLocalUser) return;
        if (!RestrainiteMod.IsRestricted(PreventionType.TrackerMovementSpeed)) return;
        var speed = RestrainiteMod.GetLowestFloat(PreventionType.TrackerMovementSpeed);
        if (float.IsNaN(speed)) return;

        if (!isActive) return;
        if (!SmoothingFilters.TryGetValue(node, out var smoothingFilter) || smoothingFilter == null)
            SmoothingFilters.Add(node, smoothingFilter = new SmoothingFilter());
        smoothingFilter.Smooth(ref position, ref rotation, __instance.Time.Delta * speed);
    }

    /*
     * This looks like a bug in Resonite, that it doesn't add a PoseFilter to trackers.
     * So we are adding one.
     */
    [HarmonyPostfix]
    [HarmonyPatch(typeof(TrackerController), "OnInputDeviceAdded")]
    private static void TrackerController_OnInputDeviceAdded_Postfix(
        TrackerController __instance,
        IInputDevice obj)
    {
        if (obj is not ITracker tracker)
            return;
        __instance.RunSynchronously(() =>
        {
            var localUser = __instance.LocalUser;
            var slot1 = localUser.Root.Slot.FindChild(tracker.PublicID);
            if (slot1 == null) return;
            var devicePositioner = slot1.GetComponent<TrackedDevicePositioner>();
            if (devicePositioner == null) return;
            if (devicePositioner.PoseFilter.Target != null) return;
            devicePositioner.PoseFilter.Target = localUser.Root.GetRegisteredComponent<UserPoseController>();
        });
    }

    private class SmoothingFilter
    {
        private bool _intermediateInitialized;
        private float3 _intermediatePosition;
        private floatQ _intermediateRotation;
        private float3 _previousPosition;
        private floatQ _previousRotation;

        internal void Smooth(ref float3 position, ref floatQ rotation, float speed)
        {
            if (!_intermediateInitialized)
            {
                _previousPosition = position;
                _intermediatePosition = position;
                _previousRotation = rotation;
                _intermediateRotation = rotation;
                _intermediateInitialized = true;
            }

            _previousPosition = MathX.SmoothLerp(_previousPosition, position,
                ref _intermediatePosition, speed);
            position = _previousPosition;

            _previousRotation = MathX.SmoothSlerp(_previousRotation, rotation,
                ref _intermediateRotation, speed);
            rotation = _previousRotation;
        }
    }
}