using HarmonyLib;
using ModelShark;
using QudJP.Localization;

namespace QudJP.Patches
{
    /// <summary>
    /// Preprocess tooltip parameter values BEFORE TooltipManager.SetTextAndSize lays out the UI.
    /// This avoids post-layout mutations causing clipping/empty renders and ensures sizing matches
    /// the finally displayed strings.
    /// </summary>
    [HarmonyPatch(typeof(TooltipManager))]
    internal static class TooltipPreprocessPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(TooltipManager.SetTextAndSize))]
        private static void BeforeSetTextAndSize(TooltipTrigger trigger)
        {
            var fields = trigger?.parameterizedTextFields;
            if (fields == null)
            {
                return;
            }

            var style = trigger?.tooltipStyle != null ? trigger.tooltipStyle.name : string.Empty;

            for (int i = 0; i < fields.Count; i++)
            {
                var f = fields[i];
                if (f == null)
                {
                    continue;
                }

                var name = f.name ?? string.Empty;
                var text = f.value ?? string.Empty;

                // Values here are often already RTF-encoded by Look.cs (RTF.FormatToRTF(...)).
                // If it looks like RTF, avoid mutations here and let TooltipRenderGuardPatch
                // operate on the already-rendered TMP text instead.
                bool isRtf = LooksLikeRtf(text);

                // Normalize ubiquitous tokens first (unburnt etc.).
                if (!isRtf)
                {
                    text = TextMeshTranslationPatchProxy.ReplaceEmbeddedTokens(text);
                }

                // Apply line-wise localization for multiline fields and specific parameter names.
                if (!isRtf && string.Equals(name, "LongDescription", System.StringComparison.Ordinal))
                {
                    text = TooltipTextLocalizer.ApplyLongDescription(text);
                }
                else if (!isRtf && string.Equals(name, "ConText", System.StringComparison.Ordinal))
                {
                    text = TooltipTextLocalizer.ApplySubHeader(text);
                }
                else if (!isRtf && string.Equals(name, "WoundLevel", System.StringComparison.Ordinal))
                {
                    text = TooltipTextLocalizer.ApplyWoundLevel(text) ?? text;
                }

                // Dictionary pass for simple one-liners (DisplayName etc.)
                if (!isRtf)
                {
                    var contextKey = !string.IsNullOrEmpty(style) ? ($"ModelShark.Tooltip.{style}.{name}") : "Look.TooltipLine";
                    text = Translator.Instance.Apply(text, contextKey);
                }

                // Persist the change
                f.value = text;
            }
        }

        /// <summary>
        /// Accessor to the token replacer in TextMeshTranslationPatch without creating a hard dependency.
        /// </summary>
        private static class TextMeshTranslationPatchProxy
        {
            public static string ReplaceEmbeddedTokens(string value)
            {
                // Keep logic in sync with TextMeshTranslationPatch.ReplaceEmbeddedTokens.
                if (string.IsNullOrEmpty(value)) return value;
                if (value.IndexOf("(unburnt)", System.StringComparison.Ordinal) >= 0)
                {
                    value = value.Replace("(unburnt)", "・未点火・");
                }
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

        private static bool LooksLikeRtf(string s)
        {
            if (string.IsNullOrEmpty(s)) return false;
            // Heuristics: RTF sequences commonly include braces and backslash commands.
            // Qud's RTF utility typically emits lots of '\\' and tokens like '\\par', '\\b', etc.
            if (s.Length > 0 && s[0] == '{') return true;
            if (s.IndexOf("\\par", System.StringComparison.Ordinal) >= 0) return true;
            if (s.IndexOf("\\b", System.StringComparison.Ordinal) >= 0) return true;
            if (s.IndexOf("\\i", System.StringComparison.Ordinal) >= 0) return true;
            if (s.IndexOf("\\cf", System.StringComparison.Ordinal) >= 0) return true;
            if (s.IndexOf("{\\", System.StringComparison.Ordinal) >= 0) return true;
            return false;
        }
    }
}

