using HarmonyLib;
using Qud.UI;
using QudJP.Localization;
using XRL.UI;

namespace QudJP.Patches
{
    [HarmonyPatch(typeof(Options))]
    internal static class OptionsLocalizationPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("LoadOptionNode")]
        private static void Postfix(ref GameOption __result)
        {
            if (__result == null)
            {
                return;
            }

            var optionId = __result.ID ?? string.Empty;
            var titleFallback = __result.DisplayText ?? optionId;
            __result.DisplayText = HelpOptionsKeybindsLocalization.LocalizeOptionTitle(optionId, titleFallback);

            if (!string.IsNullOrEmpty(__result.HelpText))
            {
                __result.HelpText = HelpOptionsKeybindsLocalization.LocalizeOptionHelp(optionId, __result.HelpText);
            }

            var categoryLabel = HelpOptionsKeybindsLocalization.LocalizeOptionCategoryLabel(
                __result.Category ?? string.Empty,
                __result.Category ?? string.Empty);

            if (__result.DisplayValues != null && __result.DisplayValues.Length > 0)
            {
                if (ReferenceEquals(__result.DisplayValues, __result.Values))
                {
                    __result.DisplayValues = (string[])__result.DisplayValues.Clone();
                }

                for (var i = 0; i < __result.DisplayValues.Length; i++)
                {
                    var value = __result.DisplayValues[i];
                    var key = string.IsNullOrWhiteSpace(value) ? $"Index{i}" : value!;
                    __result.DisplayValues[i] = HelpOptionsKeybindsLocalization.LocalizeOptionValue(
                        optionId,
                        value ?? string.Empty,
                        key);
                }
            }

            __result.SearchKeywords = HelpOptionsKeybindsLocalization.BuildOptionSearchKeywords(new[]
            {
                __result.SearchKeywords,
                __result.DisplayText,
                categoryLabel
            });
        }
    }

    [HarmonyPatch(typeof(OptionsCategoryControl))]
    internal static class OptionsCategoryControlLocalizationPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(OptionsCategoryControl.Render))]
        private static void Postfix(OptionsCategoryControl __instance)
        {
            if (__instance?.data == null || __instance.title == null)
            {
                return;
            }

            var categoryId = __instance.data.CategoryId ?? string.Empty;
            var label = HelpOptionsKeybindsLocalization.LocalizeOptionCategoryLabel(
                categoryId,
                __instance.data.Title ?? categoryId);
            __instance.title.SetText($"{{{{C|{label}}}}}");
        }
    }

    [HarmonyPatch(typeof(OptionsScreen))]
    internal static class OptionsScreenFilterLocalizationPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(OptionsScreen.FilterItems))]
        private static void Prefix(OptionsScreen __instance)
        {
            if (__instance?.menuItems == null)
            {
                return;
            }

            foreach (var row in __instance.menuItems)
            {
                if (row == null || row is OptionsCategoryRow)
                {
                    continue;
                }

                var categoryId = row.CategoryId ?? string.Empty;
                var localizedCategory = HelpOptionsKeybindsLocalization.LocalizeOptionCategoryLabel(
                    categoryId,
                    categoryId);

                row.SearchWords = HelpOptionsKeybindsLocalization.BuildOptionSearchKeywords(new[]
                {
                    row.SearchWords,
                    row.Title,
                    localizedCategory
                });
            }
        }
    }
}
