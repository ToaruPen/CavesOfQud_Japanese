using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using QudJP.Localization;
using TMPro;
using UnityEngine;
using XRL.UI;

namespace QudJP.Patches
{
    /// <summary>
    /// TextMeshPro の SetText 呼び出しを横取りし、辞書で置換する。
    /// </summary>
    [HarmonyPatch(typeof(TMP_Text))]
    internal static class TextMeshTranslationPatch
    {
        private static int EmptySetTextLogs;
        private static readonly HashSet<string> FallbackTargets = new(StringComparer.OrdinalIgnoreCase)
        {
            "Header",
            "Row",
            "Description",
            "quote",
            "attribution",
        };

        [HarmonyPrefix]
        [HarmonyPatch("text", MethodType.Setter)]
        private static void TranslateTextProperty(TMP_Text __instance, ref string value)
        {
            TranslateString(__instance, ref value);
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(TMP_Text.SetText), typeof(string))]
        [HarmonyPatch(nameof(TMP_Text.SetText), typeof(string), typeof(bool))]
        [HarmonyPatch(nameof(TMP_Text.SetText), typeof(string), typeof(float))]
        [HarmonyPatch(nameof(TMP_Text.SetText), typeof(string), typeof(float), typeof(float))]
        [HarmonyPatch(nameof(TMP_Text.SetText), typeof(string), typeof(float), typeof(float), typeof(float))]
        [HarmonyPatch(nameof(TMP_Text.SetText), typeof(string), typeof(float), typeof(float), typeof(float), typeof(float))]
        [HarmonyPatch(nameof(TMP_Text.SetText), typeof(string), typeof(float), typeof(float), typeof(float), typeof(float), typeof(float))]
        [HarmonyPatch(nameof(TMP_Text.SetText), typeof(string), typeof(float), typeof(float), typeof(float), typeof(float), typeof(float), typeof(float))]
        [HarmonyPatch(nameof(TMP_Text.SetText), typeof(string), typeof(float), typeof(float), typeof(float), typeof(float), typeof(float), typeof(float), typeof(float))]
        [HarmonyPatch(nameof(TMP_Text.SetText), typeof(string), typeof(float), typeof(float), typeof(float), typeof(float), typeof(float), typeof(float), typeof(float), typeof(float))]
        private static void TranslateString(TMP_Text __instance, ref string sourceText)
        {
            TryRestoreFallbackText(__instance, ref sourceText);
            if (string.IsNullOrEmpty(sourceText))
            {
                if (EmptySetTextLogs < 32)
                {
                    EmptySetTextLogs++;
                    var host = __instance != null ? __instance.gameObject?.name : "<null>";
                    var type = __instance?.GetType().FullName ?? "<unknown>";
                    UnityEngine.Debug.LogWarning($"[QudJP] TMP_Text.SetText empty (instance={host}, type={type})");
                }
                return;
            }

            var contextId = __instance?.GetType().FullName;
            sourceText = Translator.Instance.Apply(sourceText, contextId);
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(TMP_Text.SetText), typeof(StringBuilder))]
        [HarmonyPatch(nameof(TMP_Text.SetText), typeof(char[]))]
        [HarmonyPatch(nameof(TMP_Text.SetText), typeof(StringBuilder), typeof(int), typeof(int))]
        [HarmonyPatch(nameof(TMP_Text.SetText), typeof(char[]), typeof(int), typeof(int))]
        private static void TranslateBuilder(TMP_Text __instance, object sourceText)
        {
            if (__instance == null)
            {
                return;
            }
            switch (sourceText)
            {
                case null:
                    return;
                case StringBuilder sb:
                    if (sb.Length == 0)
                    {
                        return;
                    }

                    var translated = Translator.Instance.Apply(sb.ToString(), __instance?.GetType().FullName);
                    if (!string.Equals(sb.ToString(), translated, System.StringComparison.Ordinal))
                    {
                        sb.Clear();
                        sb.Append(translated);
                    }
                    break;
                case char[] chars:
                    if (chars.Length == 0)
                    {
                        return;
                    }

                    var original = new string(chars);
                    var result = Translator.Instance.Apply(original, __instance?.GetType().FullName);
                    if (!string.Equals(original, result, System.StringComparison.Ordinal))
                    {
                        var copyLength = Mathf.Min(chars.Length, result.Length);
                        result.CopyTo(0, chars, 0, copyLength);
                        if (copyLength < chars.Length)
                        {
                            for (var i = copyLength; i < chars.Length; i++)
                            {
                                chars[i] = '\0';
                            }
                        }
                    }
                    break;
            }
        }

        private static void TryRestoreFallbackText(TMP_Text instance, ref string text)
        {
            if (!string.IsNullOrEmpty(text) || instance == null)
            {
                return;
            }

            var name = instance.gameObject != null ? instance.gameObject.name : string.Empty;
            if (string.IsNullOrEmpty(name) || !FallbackTargets.Contains(name))
            {
                return;
            }

            var skin = instance.GetComponent<UITextSkin>();
            if (skin == null || string.IsNullOrEmpty(skin.text))
            {
                return;
            }

            text = Translator.Instance.Apply(skin.text, instance.GetType().FullName);
        }
    }
}
