using System;
using System.Collections.Generic;
using System.Diagnostics;
using HarmonyLib;
using QudJP.Diagnostics;
using QudJP.Localization;
using TMPro;
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
        private static void TranslateUITextSkin(UITextSkin __instance, ref string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                LogEmptyUITextSkin();
                return;
            }

            var tmp = __instance != null ? __instance.GetComponent<TMP_Text>() : null;
            var contextId = tmp != null
                ? (ContextHints.Resolve(tmp) ?? $"TMP.{tmp.gameObject?.name ?? "Field"}")
                : "UITextSkin.SetText";
            var eid = UIContext.Resolve(tmp);
            if (!string.IsNullOrEmpty(contextId))
            {
                if (contextId.StartsWith("ModelShark.Tooltip.", StringComparison.OrdinalIgnoreCase) ||
                    contextId.StartsWith("TMP.PopupMessage.", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
            }
            if (TranslationContextGuards.ShouldSkipTranslation(contextId, eid, text))
            {
                return;
            }

            text = Translator.Instance.Apply(text, contextId);
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
