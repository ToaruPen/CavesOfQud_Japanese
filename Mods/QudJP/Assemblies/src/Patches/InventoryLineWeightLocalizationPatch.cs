using HarmonyLib;
using Qud.UI;
using TMPro;
using UnityEngine;
using XRL.UI;
using XRL.UI.Framework;

namespace QudJP.Patches
{
    /// <summary>
    /// Rewrites weight summaries so inventory panes display Japanese units.
    /// </summary>
    [HarmonyPatch(typeof(InventoryLine))]
    internal static class InventoryLineWeightLocalizationPatch
    {
        private static int LoggedWeights;
        private const int MaxWeightLogs = 40;

        [HarmonyPostfix]
        [HarmonyPatch(nameof(InventoryLine.setData))]
        private static void LocalizeWeights(
            InventoryLine __instance,
            FrameworkDataElement data)
        {
            if (__instance == null || data is not InventoryLineData lineData)
            {
                return;
            }

            if (lineData.category)
            {
                if (__instance.categoryWeightText != null)
                {
                    var text = Options.ShowNumberOfItems
                        ? $"|{lineData.categoryAmount} 個|{FormatPounds(lineData.categoryWeight)}|"
                        : $"|{FormatPounds(lineData.categoryWeight)}|";

                    __instance.categoryWeightText.SetText(text);
                    LogWeight(__instance.categoryWeightText, "category", text, lineData.categoryName ?? "<null>");
                }

                __instance.itemWeightText?.SetText(string.Empty);
                return;
            }

            var go = lineData.go;
            if (go != null && __instance.itemWeightText != null)
            {
                var text = $"[{FormatPounds(go.Weight)}]";
                __instance.itemWeightText.SetText(text);
                LogWeight(__instance.itemWeightText, "item", text, go.Blueprint ?? go.DebugName ?? "<null>");
            }
        }

        private static string FormatPounds(int value) => $"{value} ポンド";

        private static void LogWeight(UITextSkin skin, string kind, string text, string context)
        {
            if (LoggedWeights >= MaxWeightLogs)
            {
                return;
            }

            LoggedWeights++;
            var tmp = skin.GetComponent<TMP_Text>();
            var rendered = tmp != null ? Short(tmp.text) : "<no-tmp>";
            UnityEngine.Debug.Log($"[QudJP][Diag] Inventory weight {kind}: host='{skin.gameObject?.name ?? "<null>"}' ctx='{context}' raw='{Short(text)}' tmp='{rendered}'");
        }

        private static string Short(string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return "<empty>";
            }

            return value!.Length > 80 ? value.Substring(0, 80) + "..." : value;
        }
    }
}
