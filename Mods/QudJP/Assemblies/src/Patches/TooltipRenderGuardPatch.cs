using HarmonyLib;
using ModelShark;
using TMPro;
using UnityEngine.UI;
using QudJP.Localization;

namespace QudJP.Patches
{
    /// <summary>
    /// Ensures tooltip TMP text fields are configured to not disappear due to wrapping/overflow.
    /// Applied after TooltipManager.SetTextAndSize.
    /// </summary>
    [HarmonyPatch(typeof(TooltipManager))]
    internal static class TooltipRenderGuardPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(TooltipManager.SetTextAndSize))]
        private static void AfterSetTextAndSize(TooltipTrigger trigger)
        {
            var tooltip = trigger?.Tooltip;
            if (tooltip?.TMPFields == null)
            {
                return;
            }

            // Build a quick lookup for parameterized values by field name to restore empties
            var paramMap = new System.Collections.Generic.Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase);
            if (trigger?.parameterizedTextFields != null)
            {
                foreach (var f in trigger.parameterizedTextFields)
                {
                    if (f != null && !string.IsNullOrEmpty(f.name))
                    {
                        paramMap[f.name] = f.value ?? string.Empty;
                    }
                }
            }

            foreach (var field in tooltip.TMPFields)
            {
                var t = field?.Text;
                if (t == null) continue;
                t.extraPadding = true;
                t.textWrappingMode = TextWrappingModes.PreserveWhitespace;
                t.overflowMode = TextOverflowModes.Overflow;
                var c = t.color; c.a = 1f; t.color = c;

                // Final guard: if text is empty, try to restore from parameterized value
                if (string.IsNullOrWhiteSpace(t.text))
                {
                    var name = t.gameObject != null ? t.gameObject.name : null;
                    if (!string.IsNullOrEmpty(name) && paramMap.TryGetValue(name, out var value) && !string.IsNullOrEmpty(value))
                    {
                        var style = trigger?.tooltipStyle != null ? trigger.tooltipStyle.name : "<null>";
                        var translated = Translator.Instance.Apply(value, "ModelShark.Tooltip." + style + "." + name);
                        t.text = translated;
                        UnityEngine.Debug.Log($"[QudJP] Tooltip fill {style} obj='{name}' restored from params");
                    }
                }
            }

            // Apply JP font to legacy Text fields as well so Japanese glyphs render.
            if (tooltip.TextFields != null)
            {
                foreach (var tf in tooltip.TextFields)
                {
                    Text t = tf?.Text;
                    if (t == null) continue;
                    FontManager.Instance.ApplyToLegacyText(t);
                    if (string.IsNullOrWhiteSpace(t.text))
                    {
                        var name = t.gameObject != null ? t.gameObject.name : null;
                        if (!string.IsNullOrEmpty(name) && paramMap.TryGetValue(name, out var value) && !string.IsNullOrEmpty(value))
                        {
                            var style = trigger?.tooltipStyle != null ? trigger.tooltipStyle.name : "<null>";
                            var translated = Translator.Instance.Apply(value, "ModelShark.Tooltip." + style + "." + name);
                            t.text = translated;
                            UnityEngine.Debug.Log($"[QudJP] Tooltip fill {style} obj='{name}' (legacy) restored from params");
                        }
                    }
                }
            }

            // Also walk all text components under the tooltip and translate static labels
            // that are not parameterized (do not contain the delimiter, usually "%").
            var delimiter = Tooltip.Delimiter ?? TooltipManager.Instance?.textFieldDelimiter ?? "%";

            var tmpAll = tooltip.GameObject.GetComponentsInChildren<TextMeshProUGUI>(includeInactive: true);
            foreach (var t in tmpAll)
            {
                if (t == null) continue;
                t.extraPadding = true;
                t.textWrappingMode = TextWrappingModes.PreserveWhitespace;
                t.overflowMode = TextOverflowModes.Overflow;
                var c = t.color; c.a = 1f; t.color = c;

                var current = t.text;
                var name = t.gameObject != null ? t.gameObject.name : string.Empty;

                // If SubHeader is blank, hide it to prevent a large empty row.
                if (string.IsNullOrWhiteSpace(current) && string.Equals(name, "SubHeader", System.StringComparison.Ordinal))
                {
                    t.gameObject.SetActive(false);
                    continue;
                }

                if (string.IsNullOrEmpty(current) || current.Contains(delimiter)) continue;
                var translated = Translator.Instance.Apply(current, t.GetType().FullName);
                if (!string.IsNullOrEmpty(translated) && !string.Equals(translated, current))
                {
                    t.text = translated;
                }
            }

            var textAll = tooltip.GameObject.GetComponentsInChildren<Text>(includeInactive: true);
            foreach (var t in textAll)
            {
                if (t == null) continue;
                FontManager.Instance.ApplyToLegacyText(t);
                var current = t.text;
                if (string.IsNullOrEmpty(current) || current.Contains(delimiter)) continue;
                var translated = Translator.Instance.Apply(current, t.GetType().FullName);
                if (!string.IsNullOrEmpty(translated) && !string.Equals(translated, current))
                {
                    t.text = translated;
                }
            }
        }
    }
}
