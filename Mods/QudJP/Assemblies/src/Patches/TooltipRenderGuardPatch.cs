using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using HarmonyLib;
using ModelShark;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using QudJP.Diagnostics;
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
            if (trigger == null)
            {
                return;
            }

            var tooltips = new List<Tooltip>();
            foreach (var tooltip in TooltipTraversal.EnumerateAll(trigger))
            {
                if (tooltip != null)
                {
                    tooltips.Add(tooltip);
                }
            }

            if (tooltips.Count == 0)
            {
                return;
            }

            var paramMap = BuildParameterMap(trigger);
            var fallbackStyle = trigger.tooltipStyle != null ? trigger.tooltipStyle.name : string.Empty;

            if (!string.IsNullOrEmpty(fallbackStyle) &&
                fallbackStyle.IndexOf("Dual", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                CopyIfMissing(paramMap, "DisplayName", "DisplayName2");
                CopyIfMissing(paramMap, "ConText", "ConText2");
                CopyIfMissing(paramMap, "LongDescription", "LongDescription2");
                CopyIfMissing(paramMap, "WoundLevel", "WoundLevel2");
            }

            foreach (var tooltip in tooltips)
            {
                var styleName = TooltipTraversal.ResolveStyleName(tooltip) ?? fallbackStyle;
                ProcessTooltipInstance(tooltip, trigger, styleName, paramMap);
            }

            TooltipParamMapCache.Remember(UIContext.Resolve(trigger), paramMap);
        }

        private static Dictionary<string, string> BuildParameterMap(TooltipTrigger trigger)
        {
            var paramMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (trigger?.parameterizedTextFields == null)
            {
                return paramMap;
            }

            foreach (var f in trigger.parameterizedTextFields)
            {
                if (f == null || string.IsNullOrEmpty(f.name))
                {
                    continue;
                }

                var key = f.name;
                var val = f.value ?? string.Empty;
                paramMap[key] = val;

                if (string.Equals(key, "ConText", StringComparison.OrdinalIgnoreCase))
                {
                    paramMap["SubHeader"] = val;
                }
                else if (string.Equals(key, "ConText2", StringComparison.OrdinalIgnoreCase))
                {
                    paramMap["SubHeader2"] = val;
                }

                if (key.EndsWith("2", StringComparison.Ordinal))
                {
                    var without = key.Substring(0, key.Length - 1);
                    if (!paramMap.ContainsKey(without)) paramMap[without] = val;
                }
                else
                {
                    var with2 = key + "2";
                    if (!paramMap.ContainsKey(with2)) paramMap[with2] = val;
                }

                if (!string.IsNullOrEmpty(val))
                {
                    foreach (var alias in TooltipGuardHelper.CandidatesFromName(key))
                    {
                        if (string.IsNullOrEmpty(alias) ||
                            string.Equals(alias, key, StringComparison.OrdinalIgnoreCase) ||
                            paramMap.ContainsKey(alias))
                        {
                            continue;
                        }

                        paramMap[alias] = val;
                    }
                }
            }

            return paramMap;
        }

        private static void ProcessTooltipInstance(
            Tooltip tooltip,
            TooltipTrigger trigger,
            string styleName,
            Dictionary<string, string> paramMap)
        {
            if (tooltip == null)
            {
                return;
            }

            var processedTmps = new HashSet<TMP_Text>();
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

                    TooltipPlaceholderRestorer.TryRestoreText(t, paramMap);

                    if (string.IsNullOrWhiteSpace(t.text))
                    {
                        var name = t.gameObject != null ? t.gameObject.name : null;
                        if (!string.IsNullOrEmpty(name) && paramMap.TryGetValue(name, out var value) && !string.IsNullOrEmpty(value))
                        {
                            var style = !string.IsNullOrEmpty(styleName) ? styleName : "<null>";
                            var translated = SafeStringTranslator.SafeTranslate(value, "ModelShark.Tooltip." + style + "." + name);
                            t.text = translated;
                            UnityEngine.Debug.Log($"[QudJP] Tooltip fill {style} obj='{name}' (legacy) restored from params");
                        }
                    }
                }
            }

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
                TooltipPlaceholderRestorer.TryRestoreText(t, paramMap);
                current = t.text;
                var name = t.gameObject != null ? t.gameObject.name : string.Empty;
                var rt = t.rectTransform; var rect = rt != null ? rt.rect : new UnityEngine.Rect();
                if (!string.IsNullOrEmpty(current) && rt != null && (rect.width < 1f || rect.height < 1f))
                {
                    EnsureRectSize(t);
                    var newRect = rt.rect;
                    UnityEngine.Debug.Log($"[QudJP][Diag] Tooltip tiny rect style='{(!string.IsNullOrEmpty(styleName)?styleName:"<null>")}' obj='{name}' len={current.Length} rect={rect.width:F1}x{rect.height:F1} -> {newRect.width:F1}x{newRect.height:F1}");
                }

                if (string.IsNullOrWhiteSpace(current) && !string.IsNullOrEmpty(name))
                {
                    foreach (var key in TooltipGuardHelper.CandidatesFromName(name))
                    {
                        if (string.IsNullOrEmpty(key))
                        {
                            continue;
                        }

                        if (paramMap.TryGetValue(key, out var val) && !string.IsNullOrEmpty(val))
                        {
                            t.text = SafeStringTranslator.SafeTranslate(val, "ModelShark.Tooltip." + (!string.IsNullOrEmpty(styleName)?styleName:"<null>") + "." + name);
                            current = t.text;
                            UnityEngine.Debug.Log($"[QudJP] Tooltip heuristic fill obj='{name}' <- '{key}'");
                            break;
                        }
                    }
                }

                if (string.IsNullOrWhiteSpace(current) &&
                    (string.Equals(name, "SubHeader", StringComparison.Ordinal) ||
                     string.Equals(name, "ConText", StringComparison.Ordinal)))
                {
                    t.text = string.Empty;
                    continue;
                }

                if (!string.IsNullOrEmpty(current))
                {
                    string style = !string.IsNullOrEmpty(styleName) ? styleName : "<null>";
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
                            if (string.Equals(result, segment, StringComparison.Ordinal))
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

                    current = Regex.Replace(current, @"\s*<[A-Z]{1,3}\d{1,3}>", string.Empty);

                    var normalized = TooltipTextLocalizer.ApplyLongDescription(current);
                    if (!string.Equals(normalized, current, StringComparison.Ordinal))
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
                    string style = !string.IsNullOrEmpty(styleName) ? styleName : "<null>";
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
                            if (string.Equals(result, segment, StringComparison.Ordinal))
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

                    current = Regex.Replace(current, @"\s*<[A-Z]{1,3}\d{1,3}>", string.Empty);

                    var normalized = TooltipTextLocalizer.ApplyLongDescription(current);
                    if (!string.Equals(normalized, current, StringComparison.Ordinal))
                    {
                        t.text = normalized;
                        current = normalized;
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

        private static void CopyIfMissing(
            System.Collections.Generic.Dictionary<string, string> map,
            string source,
            string target)
        {
            if (map == null)
            {
                return;
            }

            if (map.ContainsKey(target))
            {
                return;
            }

            if (map.TryGetValue(source, out var value) && !string.IsNullOrEmpty(value))
            {
                map[target] = value;
            }
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
