using System.Collections.Generic;
using HarmonyLib;
using ModelShark;
using QudJP.Diagnostics;
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
        private static void BeforeSetTextAndSize(TooltipTrigger trigger, ref string __state)
        {
            var eid = UIContext.Capture(JpLog.NewEID());
            __state = eid;
            UIContext.Bind(trigger, eid);

            var styleName = trigger?.tooltipStyle != null ? trigger.tooltipStyle.name : "<null>";
            JpLog.Info(eid, "Tooltip", "START", $"style={styleName} trigger={trigger?.name ?? "<null>"}");

            var source = TooltipSourceContext.Describe(trigger);
            if (!string.IsNullOrEmpty(source))
            {
                JpLog.Info(eid, "Tooltip", "SOURCE", source!);
            }

            TooltipSourceContext.TryGetSubjects(trigger, out var primarySubject, out var compareSubject);

            var fields = trigger?.parameterizedTextFields;
            if (fields == null)
            {
                return;
            }

            var style = styleName ?? string.Empty;
            var snapshot = new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase);

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
                else if (!isRtf && string.Equals(name, "ConText2", System.StringComparison.Ordinal))
                {
                    text = TooltipTextLocalizer.ApplySubHeader(text);
                }
                else if (!isRtf && string.Equals(name, "WoundLevel", System.StringComparison.Ordinal))
                {
                    text = TooltipTextLocalizer.ApplyWoundLevel(text) ?? text;
                }

                if (string.IsNullOrWhiteSpace(text) && IsSubHeaderField(name))
                {
                    var subject = IsSecondaryField(name) ? compareSubject : primarySubject;
                    var fallback = TooltipSubHeaderBuilder.Build(subject);
                    if (!string.IsNullOrWhiteSpace(fallback))
                    {
                        text = fallback!;
                        JpLog.Info(eid, "Tooltip", "SubHeader/FILL", $"field={name} value='{fallback}'");
                    }
                }

                // Dictionary pass for simple one-liners (DisplayName etc.)
                if (!isRtf)
                {
                    var contextKey = !string.IsNullOrEmpty(style) ? ($"ModelShark.Tooltip.{style}.{name}") : "Look.TooltipLine";
                    text = SafeStringTranslator.SafeTranslate(text, contextKey);
                }

                // Persist the change
                f.value = text;
                JpLog.Info(eid, "Tooltip", "Param", $"{f.name ?? "<null>"} len={text?.Length ?? 0}");

                if (!string.IsNullOrEmpty(name))
                {
                    snapshot[name] = text ?? string.Empty;
                }

                if (!string.IsNullOrEmpty(text))
                {
                    foreach (var alias in TooltipGuardHelper.CandidatesFromName(name))
                    {
                        if (string.IsNullOrEmpty(alias) ||
                            string.Equals(alias, name, System.StringComparison.OrdinalIgnoreCase) ||
                            snapshot.ContainsKey(alias))
                        {
                            continue;
                        }

                        snapshot[alias] = text;
                    }
                }
            }

            TooltipBodyTextCache.MergeInto(snapshot, trigger);
            TooltipParamMapCache.Remember(eid, snapshot);
        }

        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        [HarmonyPatch(nameof(TooltipManager.SetTextAndSize))]
        private static void AfterSetTextAndSize(TooltipTrigger trigger, string __state)
        {
            if (string.IsNullOrEmpty(__state))
            {
                return;
            }

            JpLog.Info(__state, "Tooltip", "END", $"trigger={trigger?.name ?? "<null>"}");
            UIContext.Release(__state);
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
            if (string.IsNullOrEmpty(s))
            {
                return false;
            }

            // Quick exit: Unity rich text uses '<' extensively but rarely '{'.
            if (s.IndexOf('<') >= 0 && s.IndexOf('{') < 0)
            {
                return false;
            }

            // Genuine RTF almost always starts with "{\rtf"
            if (s.StartsWith("{\\rtf", System.StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // Require both braces and backslash control words to avoid matching simple "{...}" strings.
            if (s.IndexOf('{') >= 0 && s.IndexOf('}') > s.IndexOf('{') && s.IndexOf('\\') >= 0)
            {
                // Look for common RTF control words to reduce false positives.
                if (s.IndexOf("\\fonttbl", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                    s.IndexOf("\\colortbl", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                    s.IndexOf("\\stylesheet", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                    s.IndexOf("\\pard", System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsSubHeaderField(string? name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            return string.Equals(name, "ConText", System.StringComparison.Ordinal) ||
                string.Equals(name, "ConText2", System.StringComparison.Ordinal) ||
                string.Equals(name, "SubHeader", System.StringComparison.Ordinal) ||
                string.Equals(name, "SubHeader2", System.StringComparison.Ordinal);
        }

        private static bool IsSecondaryField(string? name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            return name.EndsWith("2", System.StringComparison.Ordinal);
        }
    }
}
