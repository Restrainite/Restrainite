using System;
using Elements.Core;
using FrooxEngine;
using HarmonyLib;
using Restrainite.RestrictionTypes.Base;

namespace Restrainite.Patches;

[HarmonyPatch]
internal static class ShowOrHideContextMenuItems
{
    private static bool ShouldDisableButton(ContextMenu contextMenuItem, LocaleString? label)
    {
        if (Restrictions.ShowContextMenuItems.IsRestricted)
        {
            var items = Restrictions.ShowContextMenuItems.StringSet.Value;

            var hidden = !FindInList(contextMenuItem, items, label);

            if (hidden) return true;
        }

        if (Restrictions.HideContextMenuItems.IsRestricted)
        {
            var items = Restrictions.HideContextMenuItems.StringSet.Value;

            var hidden = FindInList(contextMenuItem, items, label);

            if (hidden) return true;
        }

        Chirality? chirality = null;
        if (contextMenuItem.CurrentSummoner is InteractionHandler interactionHandler)
            chirality = interactionHandler.Side.Value;

        return Restrictions.PreventLaserTouch.IsRestricted &&
               Restrictions.PreventLaserTouch.Chirality.IsRestricted(chirality) &&
               label is { content: "Interaction.LaserEnabled" };
    }

    private static bool FindInList(IWorldElement element, ImmutableStringSet items, LocaleString? label)
    {
        foreach (var item in items)
        {
            if (label?.content == null)
            {
                if (item.Equals("null")) return true;
                continue;
            }

            if (item.Equals(label.Value.content)) return true;

            // Special case for locomotion item
            if (label.Value.isLocaleKey) continue;
            var localized = element.GetLocalized(item);
            if (localized == null) continue;
            if (label.Value.content.StartsWith(localized)) return true;
        }

        return false;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ContextMenu), "AddItem",
        [
            typeof(LocaleString), typeof(IAssetProvider<ITexture2D>), typeof(Uri), typeof(IAssetProvider<Sprite>),
            typeof(colorX?), typeof(bool)
        ],
        [
            ArgumentType.Ref, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal,
            ArgumentType.Ref, ArgumentType.Normal
        ])]
    public static void ShowOrHideContextMenuItems_ContextMenuAddItem_Postfix(LocaleString label,
        ContextMenuItem __result, ContextMenu __instance)
    {
        if (ShouldDisableButton(__instance, label)) __result.Button.Slot.ActiveSelf = false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ContextMenu), "AddToggleItem")]
    public static bool ShowOrHideContextMenuItems_ContextMenuAddToggleItem_Prefix(LocaleString trueLabel,
        LocaleString falseLabel, ContextMenu __instance)
    {
        return !(ShouldDisableButton(__instance, trueLabel) || ShouldDisableButton(__instance, falseLabel));
    }
}