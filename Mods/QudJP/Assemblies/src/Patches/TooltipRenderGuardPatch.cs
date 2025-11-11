using System;
using System.Text.RegularExpressions;
using HarmonyLib;
using ModelShark;
using TMPro;
using UnityEngine;
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
        private static readonly Regex PlaceholderRegex = new("%(?<name>[A-Za-z0-9]+)%", RegexOptions.Compiled);

        [HarmonyPostfix]
        [HarmonyPatch(nameof(TooltipManager.SetTextAndSize))]
        private static void AfterSetTextAndSize(TooltipTrigger trigger)
        {
            var tooltip = trigger?.Tooltip;
            if (tooltip == null)
            {
                return;
            }

            // Build a quick lookup for parameterized values by field name to restore empties.
            // Add a few aliases for styles that rename fields (e.g., ConText -> SubHeader).
            var paramMap = new System.Collections.Generic.Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase);
            if (trigger?.parameterizedTextFields != null)
            {
                foreach (var f in trigger.parameterizedTextFields)
                {
                    if (f != null && !string.IsNullOrEmpty(f.name))
                    {
                        var key = f.name;
                        var val = f.value ?? string.Empty;
                        paramMap[key] = val;

                        // Known aliases seen across tooltip styles
                        if (string.Equals(key, "ConText", System.StringComparison.OrdinalIgnoreCase))
                        {
                            paramMap["SubHeader"] = val;
                        }
                        else if (string.Equals(key, "ConText2", System.StringComparison.OrdinalIgnoreCase))
                        {
                            paramMap["SubHeader2"] = val;
                        }
                        // Handle styles that drop numeric suffix on object names
                        if (key.EndsWith("2", System.StringComparison.Ordinal))
                        {
                            var without = key.Substring(0, key.Length - 1);
                            if (!paramMap.ContainsKey(without)) paramMap[without] = val;
                        }
                        else
                        {
                            var with2 = key + "2";
                            if (!paramMap.ContainsKey(with2)) paramMap[with2] = val;
                        }
                    }
                }
            }

            var processedTmps = new System.Collections.Generic.HashSet<TMP_Text>();
            if (tooltip.TMPFields != null)
            {
                foreach (var field in tooltip.TMPFields)
                {
                    var t = field?.Text;
                    if (t == null) continue;
                    processedTmps.Add(t);
                    FontManager.Instance.ApplyToText(t);
                    t.extraPadding = true;
                    t.textWrappingMode = TextWrappingModes.PreserveWhitespace;
                    t.overflowMode = TextOverflowModes.Overflow;
                    var c = t.color; c.a = 1f; t.color = c;
                    EnsureRectSize(t);

                    if (!string.IsNullOrEmpty(t.text) && t.text.IndexOf('%') >= 0)
                    {
                        var restored = RestorePlaceholders(t.text, paramMap);
                        if (!string.Equals(restored, t.text, System.StringComparison.Ordinal))
                        {
                            t.text = restored;
                            t.ForceMeshUpdate(ignoreActiveState: true, forceTextReparsing: true);
                            UnityEngine.Debug.Log($"[QudJP] Tooltip placeholder restored on '{t.gameObject?.name ?? "<null>"}'");
                        }
                    }

                    // Final guard: if text is empty, try to restore from parameterized value
                    if (string.IsNullOrWhiteSpace(t.text))
                    {
                        var name = t.gameObject != null ? t.gameObject.name : null;
                        if (!string.IsNullOrEmpty(name) && paramMap.TryGetValue(name, out var value) && !string.IsNullOrEmpty(value))
                        {
                            var style = trigger?.tooltipStyle != null ? trigger.tooltipStyle.name : "<null>";
                            var translated = SafeStringTranslator.SafeTranslate(value, "ModelShark.Tooltip." + style + "." + name);
                            t.text = translated;
                            UnityEngine.Debug.Log($"[QudJP] Tooltip fill {style} obj='{name}' restored from params");
                        }
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
                            var translated = SafeStringTranslator.SafeTranslate(value, "ModelShark.Tooltip." + style + "." + name);
                            t.text = translated;
                            UnityEngine.Debug.Log($"[QudJP] Tooltip fill {style} obj='{name}' (legacy) restored from params");
                        }
                    }
                }
            }

            // Also walk all text components under the tooltip and translate static labels
            // that are not parameterized (do not contain the delimiter, usually "%").
            var delimiter = Tooltip.Delimiter ?? TooltipManager.Instance?.textFieldDelimiter ?? "%";

            var tmpAll = tooltip.GameObject.GetComponentsInChildren<TMP_Text>(includeInactive: true);
            foreach (var t in tmpAll)
            {
                if (t == null || processedTmps.Contains(t)) continue;
                FontManager.Instance.ApplyToText(t);
                t.extraPadding = true;
                t.textWrappingMode = TextWrappingModes.PreserveWhitespace;
                t.overflowMode = TextOverflowModes.Overflow;
                var c = t.color; c.a = 1f; t.color = c;
                EnsureRectSize(t);

                var current = t.text;
                var name = t.gameObject != null ? t.gameObject.name : string.Empty;
                var rt = t.rectTransform; var rect = rt != null ? rt.rect : new UnityEngine.Rect();
                if (!string.IsNullOrEmpty(current) && rt != null && (rect.width < 1f || rect.height < 1f))
                {
                    EnsureRectSize(t);
                    var newRect = rt.rect;
                    UnityEngine.Debug.Log($"[QudJP][Diag] Tooltip tiny rect style='{(trigger?.tooltipStyle!=null?trigger.tooltipStyle.name:"<null>")}' obj='{name}' len={current.Length} rect={rect.width:F1}x{rect.height:F1} -> {newRect.width:F1}x{newRect.height:F1}");
                }

                // Secondary restore: in some styles the active TMP under the tooltip is not in Tooltip.TMPFields.
                if (string.IsNullOrWhiteSpace(current) && !string.IsNullOrEmpty(name))
                {
                    // candidate keys to probe in order
                    foreach (var key in TooltipGuardHelper.CandidatesFromName(name))
                    {
                        if (string.IsNullOrEmpty(key))
                        {
                            continue;
                        }

                        if (paramMap.TryGetValue(key, out var val) && !string.IsNullOrEmpty(val))
                        {
                            t.text = SafeStringTranslator.SafeTranslate(val, "ModelShark.Tooltip." + (trigger?.tooltipStyle!=null?trigger.tooltipStyle.name:"<null>") + "." + name);
                            current = t.text;
                            UnityEngine.Debug.Log($"[QudJP] Tooltip heuristic fill obj='{name}' <- '{key}'");
                            break;
                        }
                    }
                }

                // If SubHeader/ConText is blank, hide it to prevent a large empty row.
                if (string.IsNullOrWhiteSpace(current) &&
                    (string.Equals(name, "SubHeader", System.StringComparison.Ordinal) ||
                     string.Equals(name, "ConText", System.StringComparison.Ordinal)))
                {
                    t.gameObject.SetActive(false);
                    continue;
                }

                if (!string.IsNullOrEmpty(current))
                {
                    // Try contextual translation with style+object name, then fallback to type, both with trim-aware compare
                    string style = trigger?.tooltipStyle != null ? trigger.tooltipStyle.name : "<null>";
                    string objName = t.gameObject != null ? t.gameObject.name : string.Empty;

                    string leading = string.Empty, trailing = string.Empty;
                    string toTranslate = current;
                    int l = 0, r = current.Length - 1;
                    while (l <= r && char.IsWhiteSpace(current[l])) l++;
                    while (r >= l && char.IsWhiteSpace(current[r])) r--;
                    if (l > 0) leading = current.Substring(0, l);
                    if (r < current.Length - 1) trailing = current.Substring(r + 1);
                    if (l > 0 || r < current.Length - 1)
                    {
                        toTranslate = current.Substring(l, r - l + 1);
                    }

                    var translated = TranslateWithDelimiter(
                        toTranslate,
                        delimiter,
                        segment =>
                        {
                            var result = SafeStringTranslator.SafeTranslate(segment, "ModelShark.Tooltip." + style + "." + objName);
                            if (string.Equals(result, segment, System.StringComparison.Ordinal))
                            {
                                result = SafeStringTranslator.SafeTranslate(segment, t.GetType().FullName);
                            }
                            return result;
                        });

                    if (!string.IsNullOrEmpty(translated) && !string.Equals(translated, toTranslate))
                    {
                        t.text = leading + translated + trailing;
                        current = t.text;
                        UnityEngine.Debug.Log($"[QudJP] Tooltip static translated style='{style}' obj='{objName}' -> '{translated}'");
                    }

                    // Strip stray debug markers like <AD1>, <CD24>
                    current = System.Text.RegularExpressions.Regex.Replace(current, "\\s*<[A-Z]{1,3}\\d{1,3}>", string.Empty);

                    // Apply line-wise tooltip localization patterns for remaining English fragments
                    var normalized = QudJP.Localization.TooltipTextLocalizer.ApplyLongDescription(current);
                    if (!string.Equals(normalized, current, System.StringComparison.Ordinal))
                    {
                        t.text = normalized;
                        current = normalized;
                    }
                }
                else
                {
                    continue;
                }
            }

            var textAll = tooltip.GameObject.GetComponentsInChildren<Text>(includeInactive: true);
            foreach (var t in textAll)
            {
                if (t == null) continue;
                FontManager.Instance.ApplyToLegacyText(t);
                var current = t.text;
                if (!string.IsNullOrEmpty(current))
                {
                    string style = trigger?.tooltipStyle != null ? trigger.tooltipStyle.name : "<null>";
                    string objName = t.gameObject != null ? t.gameObject.name : string.Empty;

                    string leading = string.Empty, trailing = string.Empty;
                    string toTranslate = current;
                    int l = 0, r = current.Length - 1;
                    while (l <= r && char.IsWhiteSpace(current[l])) l++;
                    while (r >= l && char.IsWhiteSpace(current[r])) r--;
                    if (l > 0) leading = current.Substring(0, l);
                    if (r < current.Length - 1) trailing = current.Substring(r + 1);
                    if (l > 0 || r < current.Length - 1)
                    {
                        toTranslate = current.Substring(l, r - l + 1);
                    }

                    var translated = TranslateWithDelimiter(
                        toTranslate,
                        delimiter,
                        segment =>
                        {
                            var result = SafeStringTranslator.SafeTranslate(segment, "ModelShark.Tooltip." + style + "." + objName);
                            if (string.Equals(result, segment, System.StringComparison.Ordinal))
                            {
                                result = SafeStringTranslator.SafeTranslate(segment, t.GetType().FullName);
                            }
                            return result;
                        });

                    if (!string.IsNullOrEmpty(translated) && !string.Equals(translated, toTranslate))
                    {
                        t.text = leading + translated + trailing;
                        current = t.text;
                    }

                    current = System.Text.RegularExpressions.Regex.Replace(current, "\\s*<[A-Z]{1,3}\\d{1,3}>", string.Empty);

                    var normalized = QudJP.Localization.TooltipTextLocalizer.ApplyLongDescription(current);
                    if (!string.Equals(normalized, current, System.StringComparison.Ordinal))
                    {
                        t.text = normalized;
                    }
                }
            }
        }

        private static void EnsureRectSize(TMP_Text t)
        {
            if (t == null)
            {
                return;
            }

            var rt = t.rectTransform;
            if (rt == null)
            {
                return;
            }

            var rect = rt.rect;
            if (rect.width >= 1f && rect.height >= 1f)
            {
                return;
            }

            var preferred = t.GetPreferredValues(Mathf.Max(8f, rect.width), Mathf.Max(8f, rect.height));
            var width = Mathf.Max(preferred.x, 64f);
            var height = Mathf.Max(preferred.y, 16f);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            LayoutRebuilder.MarkLayoutForRebuild(rt);
            LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
        }

        private static string RestorePlaceholders(string text, System.Collections.Generic.Dictionary<string, string> values)
        {
            if (string.IsNullOrEmpty(text) || values == null || values.Count == 0)
            {
                return text;
            }

            return PlaceholderRegex.Replace(
                text,
                match =>
                {
                    var key = match.Groups["name"].Value;
                    if (string.IsNullOrEmpty(key))
                    {
                        return match.Value;
                    }

                    foreach (var candidate in TooltipGuardHelper.CandidatesFromName(key))
                    {
                        if (!string.IsNullOrEmpty(candidate) &&
                            values.TryGetValue(candidate, out var value) &&
                            !string.IsNullOrWhiteSpace(value))
                        {
                            return value;
                        }
                    }

                    return match.Value;
                });
        }

        private static string TranslateWithDelimiter(string value, string delimiter, System.Func<string, string> translator)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            if (string.IsNullOrEmpty(delimiter) || value.IndexOf(delimiter, System.StringComparison.Ordinal) < 0)
            {
                return translator(value);
            }

            var parts = value.Split(new[] { delimiter }, System.StringSplitOptions.None);
            for (int i = 0; i < parts.Length; i++)
            {
                parts[i] = translator(parts[i]);
            }

            return string.Join(delimiter, parts);
        }
    }
}

namespace QudJP.Patches
{
    internal static class TooltipGuardHelper
    {
        internal static System.Collections.Generic.IEnumerable<string> CandidatesFromName(string name)
        {
            // 1) exact
            yield return name;

            // 2) simple numeric alias
            if (name.EndsWith("2", System.StringComparison.Ordinal))
            {
                yield return name.Substring(0, name.Length - 1);
            }
            else
            {
                yield return name + "2";
            }

            // 3) known alias: SubHeader <-> ConText
            if (string.Equals(name, "SubHeader", System.StringComparison.OrdinalIgnoreCase))
                yield return "ConText";
            if (string.Equals(name, "SubHeader2", System.StringComparison.OrdinalIgnoreCase))
                yield return "ConText2";

            // 4) normalize variants like LongDescriptionLeft/Right/NameR/DescL -> DisplayName/LongDescription (+2 for right)
            var lower = name.ToLowerInvariant();
            bool isRight = lower.Contains("right") || lower.EndsWith("r") || lower.EndsWith("2");
            string baseField = null;
            if (lower.Contains("display") || lower.Contains("name") || lower.Contains("title")) baseField = "DisplayName";
            else if (lower.Contains("desc") || lower.Contains("descr")) baseField = "LongDescription";
            else if (lower.Contains("wound")) baseField = "WoundLevel";
            else if (lower.Contains("context") || lower.Contains("subheader") || lower.Contains("subtitle")) baseField = "ConText";

            if (baseField != null)
            {
                yield return baseField + (isRight ? "2" : string.Empty);
            }
        }
    }
}
