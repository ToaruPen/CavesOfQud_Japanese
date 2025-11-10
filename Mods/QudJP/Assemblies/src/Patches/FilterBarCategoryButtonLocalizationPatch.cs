using HarmonyLib;
using Qud.UI;
using QudJP.Localization;

namespace QudJP
{
    /// <summary>
    /// Provides localized labels for the equipment/inventory filter categories.
    /// </summary>
    [HarmonyPatch(typeof(FilterBarCategoryButton))]
    internal static class FilterBarCategoryButtonLocalizationPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(MethodType.StaticConstructor)]
        private static void InjectTranslations()
        {
            InventoryCategoryLocalization.ApplyTo(FilterBarCategoryButton.categoryTextMap);
        }

        [HarmonyPostfix]
        [HarmonyPatch("SetCategory")]
        private static void LocalizeInstance(FilterBarCategoryButton __instance, string category, string tooltip)
        {
            if (__instance?.text == null)
            {
                return;
            }

            if (InventoryCategoryLocalization.TryTranslate(category, out var translated))
            {
                __instance.text.SetText(translated);
            }
        }
    }
}
