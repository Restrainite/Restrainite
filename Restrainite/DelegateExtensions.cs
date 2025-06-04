using System;
using ResoniteModLoader;

namespace Restrainite;

internal static class DelegateExtensions
{
    internal static void SafeInvoke(this Delegate? del, params object[] args)
    {
        if (del == null) return;
        foreach (var invocation in del.GetInvocationList())
        {
            if (invocation == null) continue;
            try
            {
                invocation.Method.Invoke(invocation.Target, args);
            }
            catch (Exception ex)
            {
                ResoniteMod.Error($"{RestrainiteMod.LogReportUrl} Failed to invoke {invocation.Method.Name}: {ex}");
            }
        }
    }
}