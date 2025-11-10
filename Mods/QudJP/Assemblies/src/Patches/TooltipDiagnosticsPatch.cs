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

        [HarmonyPostfix]
        [HarmonyPatch(nameof(TooltipManager.SetTextAndSize))]
        private static void AfterSetTextAndSize(TooltipTrigger trigger)
        {
            if (trigger == null || Logged >= MaxLogs)
            {
                return;
            }

            var tooltip = trigger.Tooltip;
            if (tooltip == null)
            {
                return;
            }

            try
            {
                Logged++;
                var style = trigger.tooltipStyle != null ? trigger.tooltipStyle.name : "<null>";
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
    }
}
