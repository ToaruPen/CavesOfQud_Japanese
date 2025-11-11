using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using HarmonyLib;
using QudJP.Diagnostics;
using QudJP.Localization;
using TMPro;
using UnityEngine;
using Qud.UI;

namespace QudJP.Patches
{
    /// <summary>
    /// Intercepts TextMeshPro text assignments and applies JP translation + token normalization.
    /// </summary>
    [HarmonyPatch(typeof(TMP_Text))]
    internal static class TextMeshTranslationPatch
    {
        private static int EmptySetTextLogs;
        private static readonly HashSet<string> EmptySetTextStackLogged = new(StringComparer.OrdinalIgnoreCase);

        // Some TMP objects get their text cleared and rely on UITextSkin fallback text.
        private static readonly HashSet<string> FallbackTargets = new(StringComparer.OrdinalIgnoreCase)
        {
            "Header",
            "Row",
            "Description",
            "quote",
            "attribution",
        };

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
                    if (ShouldLogEmptyStack(host) && EmptySetTextStackLogged.Add(host ?? string.Empty))
                    {
                        var stack = new StackTrace(2, fNeedFileInfo: false);
                        UnityEngine.Debug.Log($"[QudJP] TMP_Text.SetText empty stack host='{host}' type='{type}': {stack}");
                    }
                }
                return;
            }

            var contextId = ContextHints.Resolve(__instance) ?? __instance?.GetType().FullName ?? typeof(TMP_Text).FullName;
            var eid = UIContext.Resolve(__instance);
            if (TranslationContextGuards.ShouldSkipTranslation(contextId, eid, sourceText))
            {
                return;
            }

            sourceText = Translator.Instance.Apply(sourceText, contextId);
            if (!string.IsNullOrEmpty(sourceText))
            {
                sourceText = ReplaceEmbeddedTokens(sourceText);
            }
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
                    var contextId = ContextHints.Resolve(__instance) ?? __instance?.GetType().FullName ?? typeof(TMP_Text).FullName;
                    var eid = UIContext.Resolve(__instance);
                    var original = sb.ToString();
                    if (TranslationContextGuards.ShouldSkipTranslation(contextId, eid, original))
                    {
                        return;
                    }

                    var translated = Translator.Instance.Apply(original, contextId);
                    translated = ReplaceEmbeddedTokens(translated);
                    if (!string.Equals(original, translated, StringComparison.Ordinal))
                    {
                        sb.Clear();
                        sb.Append(translated);
                    }
                    break;

                case char[] chars:
                    // 安全性のため char[] ルートの翻訳は無効化（部分コピーで文字化けを誘発するため）。
                    return;
            }
        }

        private static bool ShouldLogEmptyStack(string? host)
        {
            if (string.IsNullOrEmpty(host))
            {
                return false;
            }

            if (host.StartsWith("Target:", StringComparison.Ordinal))
            {
                return true;
            }

            switch (host)
            {
                case "Row":
                case "Header":
                case "ItemWeightText":
                case "CategoryWeightText":
                case "Resistance Attributes Details":
                case "Secondary Attributes Details":
                case "Primary Attributes Details":
                    return true;
            }

            if (host.IndexOf("Attributes Details", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            return false;
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

            // Avoid hard reference to UITextSkin to keep compile simple across game versions.
            string skinText = null;
            foreach (var comp in instance.GetComponents<UnityEngine.Component>())
            {
                if (comp == null) continue;
                var t = comp.GetType();
                if (!string.Equals(t.Name, "UITextSkin", StringComparison.Ordinal)) continue;
                var prop = t.GetProperty("text");
                skinText = prop?.GetValue(comp) as string;
                break;
            }
            if (string.IsNullOrEmpty(skinText))
            {
                return;
            }

            var contextId = ContextHints.Resolve(instance) ?? instance.GetType().FullName;
            text = Translator.Instance.Apply(skinText, contextId);
        }

        internal static string ReplaceEmbeddedTokens(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            // Simple embedded token replacements used widely across UI lines.
            if (value.IndexOf("(unburnt)", StringComparison.Ordinal) >= 0)
            {
                // Normalize common inline state markers
                value = value.Replace("(unburnt)", "・未点火・");
            }

            // Affix and difficulty tokens that appear inline in DisplayName lines
            value = System.Text.RegularExpressions.Regex.Replace(
                value,
                "\\bfreezing\\b",
                "凍結",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            value = System.Text.RegularExpressions.Regex.Replace(
                value,
                "\\bVery\\s+Low\\b",
                "非常に低い",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            return value;
        }
    }
}
