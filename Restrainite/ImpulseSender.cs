using System;
using FrooxEngine;
using FrooxEngine.ProtoFlux;
using Restrainite.RestrictionTypes.Base;

namespace Restrainite;

public class ImpulseSender
{
    private const string ImpulsePrefix = "Restrainite";
    private readonly Configuration _configuration;
    private readonly WeakReference<UserRoot> _userRoot;

    internal ImpulseSender(Configuration config, UserRoot userRoot)
    {
        _configuration = config;
        _userRoot = new WeakReference<UserRoot>(userRoot);
    }

    internal event Action<ImpulseSender>? OnDestroy;

    internal void Destroy()
    {
        OnDestroy.SafeInvoke(this);
    }

    internal void SendDynamicImpulse(IRestriction restriction, bool value, IDynamicVariableSpace source)
    {
        if (!_configuration.SendDynamicImpulses) return;
        if (!GetLocalUserSlot(out var slot) || slot == null) return;
        slot.RunSynchronously(() =>
        {
            if (slot.IsDestroyed || slot.IsDestroying) return;
            if (!_configuration.AllowRestrictionsFromWorld(slot.World, restriction)) return;
            ProtoFluxHelper.DynamicImpulseHandler.TriggerAsyncDynamicImpulseWithArgument(
                slot, $"{ImpulsePrefix} Change", true,
                $"{restriction.Name}:bool:{value}"
            );
            ProtoFluxHelper.DynamicImpulseHandler.TriggerAsyncDynamicImpulseWithArgument(
                slot, $"{ImpulsePrefix} {restriction.Name}", true,
                value
            );
        });
    }

    private bool GetLocalUserSlot(out Slot? slot)
    {
        slot = null;
        var userRootFound = _userRoot.TryGetTarget(out var userRoot);
        if (!userRootFound || userRoot == null || userRoot.IsDisposed || userRoot.IsDestroyed) return false;
        slot = userRoot.Slot;
        return slot is { IsDisposed: false, IsDestroyed: false };
    }
}