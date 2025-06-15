using System;
using FrooxEngine;
using FrooxEngine.UIX;
using HarmonyLib;
using Restrainite.RestrictionTypes.Base;

namespace Restrainite.Patches;

[HarmonyPatch]
internal static class PreventReading
{
    private static readonly byte[] Randomness = new byte[64];

    static PreventReading()
    {
        new Random().NextBytes(Randomness);
    }

    internal static void Initialize()
    {
        Restrictions.PreventReading.OnChanged += OnChanged;
    }

    private static void OnChanged(IRestriction restriction)
    {
        var texts = Engine.Current?.WorldManager?.FocusedWorld?.RootSlot?.GetComponentsInChildren<Text>();
        if (texts != null)
            foreach (var text in texts)
                text?.MarkChangeDirty();

        var textRenderers =
            Engine.Current?.WorldManager?.FocusedWorld?.RootSlot?.GetComponentsInChildren<TextRenderer>();
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
        if (!Restrictions.PreventReading.IsRestricted) return true;

        var source = value.ToCharArray();
        var length = source.Length;
        var previousChar = 0;

        // run the algorithm twice to make the first character unpredictable.
        for (var j = 0; j < 2; j++)
        {
            var insideTag = false;
            var skip = j * length;
            for (var i = 0; i < length; i++)
            {
                var character = source[i];

                // This is a basic tag detection, it will definitely break. But it's good for now.
                if (character == '<')
                    insideTag = true;
                if (character == '>')
                    insideTag = false;

                if (insideTag)
                {
                    source[i] = character;
                }
                else
                {
                    var randomValue = character + previousChar + Randomness[(i + skip) % Randomness.Length];
                    if (char.IsNumber(character))
                        source[i] = (char)(randomValue % 10 + '0');
                    else if (char.IsLower(character))
                        source[i] = (char)(randomValue % 26 + 'a');
                    else if (char.IsUpper(character))
                        source[i] = (char)(randomValue % 26 + 'A');
                    previousChar = character;
                }
            }
        }

        ____string = new string(source);
        return false;
    }
}