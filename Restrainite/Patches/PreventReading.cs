using System;
using FrooxEngine;
using FrooxEngine.UIX;
using HarmonyLib;
using Restrainite.Enums;

namespace Restrainite.Patches;

[HarmonyPatch]
internal static class PreventReading
{
    private static readonly int Random = new Random().Next();

    internal static void Initialize()
    {
        RestrainiteMod.OnRestrictionChanged += OnChange;
    }

    private static void OnChange(PreventionType preventionType, bool value)
    {
        if (preventionType != PreventionType.PreventReading)
            return;
        var texts = Engine.Current?.WorldManager?.FocusedWorld?.RootSlot?.GetComponentsInChildren<Text>();
        if (texts != null)
            foreach (var text in texts)
                text?.MarkChangeDirty();
        
        var textRenderers = Engine.Current?.WorldManager?.FocusedWorld?.RootSlot?.GetComponentsInChildren<TextRenderer>();
        if (textRenderers != null)
            foreach (var textRenderer in textRenderers)
                textRenderer?.MarkChangeDirty();

        var userspaceTexts = Userspace.UserspaceWorld?.RootSlot?.GetComponentsInChildren<Text>();
        if (userspaceTexts != null)
            foreach (var text in userspaceTexts)
                text?.MarkChangeDirty();
        
        var userspaceTextRenderers = Userspace.UserspaceWorld?.RootSlot?.GetComponentsInChildren<TextRenderer>();
        if (userspaceTextRenderers != null)
            foreach (var textRenderer in userspaceTextRenderers)
                textRenderer?.MarkChangeDirty();
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(TextRenderManager), nameof(TextRenderManager.String), MethodType.Setter)]
    private static bool TextRenderManager_String_Setter_Prefix(string value,
        ref string ____string)
    {
        if (value == null) return true;
        if (!RestrainiteMod.IsRestricted(PreventionType.PreventReading)) return true;

        var source = value.ToCharArray();
        var target = new char[source.Length];
        var i = 0;
        var insideTag = false;
        var previousChar = 0;
        foreach (var character in source)
        {
            // This is a basic tag detection, it will definitely break. But it's good for now.
            if (character == '<')
                insideTag = true;
            if (character == '>')
                insideTag = false;
            
            target[i++] = character switch
            {
                >= '0' and <= '9' => insideTag?character:(char)((character - '0' + Random + previousChar) % 10 + '0'),
                >= 'a' and <= 'z' => insideTag?character:(char)((character - 'a' + Random + previousChar) % 26 + 'a'),
                >= 'A' and <= 'Z' => insideTag?character:(char)((character - 'A' + Random + previousChar) % 26 + 'A'),
                _ => character
            };
            previousChar = character;
        }
        ____string = new string(target);
        return false;
    }
}