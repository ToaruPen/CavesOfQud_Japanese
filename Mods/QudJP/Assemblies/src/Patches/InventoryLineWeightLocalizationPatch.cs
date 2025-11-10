using HarmonyLib;
using Qud.UI;
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
                }

                __instance.itemWeightText?.SetText(string.Empty);
                return;
            }

            if (lineData.go != null && __instance.itemWeightText != null)
            {
                __instance.itemWeightText.SetText($"[{FormatPounds(lineData.go.Weight)}]");
            }
        }

        private static string FormatPounds(int value) => $"{value} ポンド";
    }
}
