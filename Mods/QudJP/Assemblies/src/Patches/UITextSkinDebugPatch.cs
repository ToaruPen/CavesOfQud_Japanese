using System;
using HarmonyLib;
using Qud.UI;
using XRL.UI;
using TMPro;
using UnityEngine;

namespace QudJP.Patches
{
    /// <summary>
    /// Diagnostic: capture UITextSkin.Apply() results for contexts where empty text has been observed
    /// (InventoryLine rows and SelectableTextMenuItem entries). Logs only a small number per run.
    /// </summary>
    [HarmonyPatch(typeof(UITextSkin))]
    internal static class UITextSkinDebugPatch
    {
        private static int Logged;
        private const int MaxLogs = 200;

        [HarmonyPostfix]
        [HarmonyPatch(nameof(UITextSkin.Apply))]
        private static void AfterApply(UITextSkin __instance)
        {
            if (Logged >= MaxLogs || __instance == null)
            {
                return;
            }

            // Capture a couple of relevant contexts
            var line = __instance.GetComponentInParent<InventoryLine>();
            var menu = __instance.GetComponentInParent<SelectableTextMenuItem>();
            if (line == null && menu == null)
                return;

            try
            {
                Logged++;
                var raw = __instance.text ?? "<null>";
                var tmp = __instance.GetComponent<TMP_Text>();
                var rendered = tmp?.text ?? "<null>";
                if (raw.Length > 200) raw = raw.Substring(0, 200) + "...";
                if (rendered.Length > 200) rendered = rendered.Substring(0, 200) + "...";

                var ctx = line != null ? "InventoryLine" : "SelectableTextMenuItem";
                string modes = "<null>", size = "<null>", color = "<null>";
                if (tmp != null)
                {
                    var c = tmp.color; color = $"rgba({c.r:F2},{c.g:F2},{c.b:F2},{c.a:F2})";
                    var rt = tmp.rectTransform; size = rt != null ? $"{rt.rect.width:F1}x{rt.rect.height:F1}" : "<none>";
                    modes = $"wrap={tmp.textWrappingMode}, overflow={tmp.overflowMode}";
                }

                Debug.Log($"[QudJP] UITextSkin.Apply ({ctx}): useBlockWrap={__instance.useBlockWrap}, blockWrap={__instance.blockWrap}, raw='{raw}' => tmp='{rendered}', modes={modes}, rect={size}, color={color}");
            }
            catch (Exception)
            {
                // best-effort diagnostics only
            }
        }
    }
}
