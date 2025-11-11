using HarmonyLib;
using QudJP.Localization;
using Qud.UI;
using XRL.UI;

namespace QudJP.Patches
{
    /// <summary>
    /// Localizes the summary labels (price/weight) at the top of the modern inventory screen.
    /// </summary>
    [HarmonyPatch(typeof(InventoryAndEquipmentStatusScreen))]
    internal static class InventoryHeaderLocalizationPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(InventoryAndEquipmentStatusScreen.UpdateViewFromData))]
        private static void LocalizeHeader(InventoryAndEquipmentStatusScreen __instance)
        {
            if (__instance == null)
            {
                return;
            }

            var go = __instance.GO;
            if (go == null)
            {
                return;
            }

            if (__instance.priceText is UITextSkin priceSkin)
            {
                var localized = InventoryLabelLocalizer.FormatPrice(go.GetFreeDrams());
                priceSkin.SetText(localized);
            }

            if (__instance.weightText is UITextSkin weightSkin)
            {
                var localized = InventoryLabelLocalizer.FormatHeaderWeight(
                    go.GetCarriedWeight(),
                    go.GetMaxCarriedWeight());
                weightSkin.SetText(localized);
            }
        }
    }
}
