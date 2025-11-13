using System;
using System.Collections.Generic;
using HarmonyLib;
using ModelShark;
using TMPro;
using UnityEngine;

namespace QudJP.Patches
{
    /// <summary>
    /// Logs the tooltip text assembly after SetTextAndSize to catch empty results.
    /// </summary>
    [HarmonyPatch(typeof(TooltipManager))]
    internal static class TooltipDiagnosticsPatch
    {
        private static int Logged;
        private const int MaxLogs = 300;
        private static readonly HashSet<int> FontReports = new();
        private const string VanillaFontToken = "SourceCodePro";

        [HarmonyPostfix]
        [HarmonyPriority(Priority.High)]
        [HarmonyPatch(nameof(TooltipManager.SetTextAndSize))]
        private static void AfterSetTextAndSize(TooltipTrigger trigger)
        {
            if (trigger == null)
            {
                return;
            }

            var tooltip = trigger.Tooltip;
            if (tooltip == null)
            {
                return;
            }

            var style = trigger.tooltipStyle != null ? trigger.tooltipStyle.name : "<null>";
            WatchFonts(tooltip, style);

            if (Logged >= MaxLogs)
            {
                return;
            }

            try
            {
                Logged++;
                string BuildPreview(string s)
                {
                    if (string.IsNullOrEmpty(s)) return "<empty>";
                    var t = s;
                    if (t.Length > 150) t = t.Substring(0, 150) + "...";
                    return t;
                }

                // Log parameter values
                if (trigger.parameterizedTextFields != null)
                {
                    foreach (var f in trigger.parameterizedTextFields)
                    {
                        Debug.Log($"[QudJP] Tooltip param {style} {f.name}='{BuildPreview(f.value)}'");
                    }
                }

                // Log resulting TMP texts
                if (tooltip.TMPFields != null)
                {
                    foreach (var field in tooltip.TMPFields)
                    {
                        var goName = field.Text != null ? field.Text.gameObject.name : "<null>";
                        var txt = field.Text?.text ?? "<null>";
                        Debug.Log($"[QudJP] Tooltip chunk {style} obj='{goName}' text='{BuildPreview(txt)}'");
                    }
                }
            }
            catch { }
        }

        private static void WatchFonts(Tooltip tooltip, string style)
        {
            if (tooltip == null)
            {
                return;
            }

            var root = tooltip.GameObject;
            if (root == null)
            {
                return;
            }

            var tmps = root.GetComponentsInChildren<TMP_Text>(includeInactive: true);
            foreach (var text in tmps)
            {
                if (text == null)
                {
                    continue;
                }

                var fontName = text.font != null ? text.font.name : string.Empty;
                if (string.IsNullOrEmpty(fontName) ||
                    fontName.IndexOf(VanillaFontToken, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }

                var id = text.GetInstanceID();
                if (!FontReports.Add(id))
                {
                    continue;
                }

                var fieldName = text.gameObject != null ? text.gameObject.name : "<null>";
                var len = text.text?.Length ?? 0;
                Debug.LogWarning($"[QudJP][FontWatch] Tooltip style={style} obj='{fieldName}' stuck on font='{fontName}' len={len}");
            }
        }
    }
}
