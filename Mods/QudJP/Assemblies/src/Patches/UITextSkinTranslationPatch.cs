using System.Collections.Generic;
using System.Diagnostics;
using HarmonyLib;
using QudJP.Localization;
using XRL.UI;

namespace QudJP.Patches
{
    /// <summary>
    /// Ensures UITextSkin.SetText calls also go through Translator so upstream markup has localized text.
    /// </summary>
    [HarmonyPatch(typeof(UITextSkin))]
    internal static class UITextSkinTranslationPatch
    {
        private const int MaxLoggedEmptyStacks = 12;
        private static readonly HashSet<string> EmptyStackLog = new();

        [HarmonyPrefix]
        [HarmonyPatch(nameof(UITextSkin.SetText), typeof(string))]
        private static void TranslateUITextSkin(ref string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                LogEmptyUITextSkin();
                return;
            }

            text = Translator.Instance.Apply(text, "UITextSkin.SetText");
        }

        private static void LogEmptyUITextSkin()
        {
            var stack = new StackTrace(2, fNeedFileInfo: false);
            var keyFrame = stack.GetFrame(0);
            var key = keyFrame != null
                ? $"{keyFrame.GetMethod()?.DeclaringType?.FullName}.{keyFrame.GetMethod()?.Name}"
                : stack.ToString();

            if (!EmptyStackLog.Add(key))
            {
                return;
            }

            if (EmptyStackLog.Count > MaxLoggedEmptyStacks)
            {
                return;
            }

            UnityEngine.Debug.LogWarning($"[QudJP] UITextSkin.SetText(empty) stack: {stack}");
        }
    }
}
