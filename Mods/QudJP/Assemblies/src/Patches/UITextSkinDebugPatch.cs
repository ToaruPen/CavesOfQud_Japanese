using System;
using HarmonyLib;
using Qud.UI;
using XRL.UI;
using TMPro;
using UnityEngine;

namespace QudJP.Patches
{
    /// <summary>
    /// Diagnostic: capture UITextSkin.Apply() result for inventory rows.
    /// Logs at most a small number of lines per session.
    /// </summary>
    [HarmonyPatch(typeof(UITextSkin))]
    internal static class UITextSkinDebugPatch
    {
        private static int Logged;
        private const int MaxLogs = 40;

        [HarmonyPostfix]
        [HarmonyPatch(nameof(UITextSkin.Apply))]
        private static void AfterApply(UITextSkin __instance)
        {
            if (Logged >= MaxLogs || __instance == null)
            {
                return;
            }

            // Only log when this UITextSkin lives under an InventoryLine row
            var line = __instance.GetComponentInParent<InventoryLine>();
            if (line == null)
            {
                return;
            }

            try
            {
                Logged++;
                var raw = __instance.text ?? "<null>";
                var tmp = __instance.GetComponent<TMP_Text>();
                var rendered = tmp?.text ?? "<null>";
                if (raw.Length > 200) raw = raw.Substring(0, 200) + "...";
                if (rendered.Length > 200) rendered = rendered.Substring(0, 200) + "...";

                Debug.Log($"[QudJP] UITextSkin.Apply (InventoryLine): raw='{raw}' => tmp='{rendered}' (obj='{__instance.gameObject?.name}')");
            }
            catch (Exception)
            {
                // best-effort diagnostics only
            }
        }
    }
}
