using HarmonyLib;
using Qud.UI;
using QudJP.Localization;
using XRL.UI.Framework;

namespace QudJP.Patches
{
    [HarmonyPatch(typeof(HelpRow))]
    internal static class HelpRowLocalizationPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(HelpRow.setData))]
        private static void Prefix(FrameworkDataElement data, out string? __state)
        {
            __state = null;
            if (data is not HelpDataRow helpDataRow)
            {
                return;
            }

            var categoryId = helpDataRow.CategoryId ?? string.Empty;
            var titleFallback = helpDataRow.Description ?? categoryId;
            var localizedTitle = HelpOptionsKeybindsLocalization.LocalizeHelpTitle(categoryId, titleFallback);
            helpDataRow.Description = localizedTitle;

            if (!string.IsNullOrEmpty(helpDataRow.HelpText))
            {
                helpDataRow.HelpText = HelpOptionsKeybindsLocalization.LocalizeHelpBody(categoryId, helpDataRow.HelpText);
            }

            __state = localizedTitle;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(HelpRow.setData))]
        private static void Postfix(HelpRow __instance, string? __state)
        {
            if (string.IsNullOrEmpty(__state) || __instance?.categoryDescription == null)
            {
                return;
            }

            __instance.categoryDescription.SetText($"{{{{C|{__state}}}}}");
        }
    }

    [HarmonyPatch(typeof(LeftSideCategory))]
    internal static class LeftSideCategoryLocalizationPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(LeftSideCategory.setData))]
        private static void Postfix(LeftSideCategory __instance, FrameworkDataElement data)
        {
            if (__instance?.text == null || data == null)
            {
                return;
            }

            string? label = null;
            switch (data)
            {
                case HelpDataRow helpDataRow:
                    label = HelpOptionsKeybindsLocalization.LocalizeHelpCategoryLabel(
                        helpDataRow.CategoryId,
                        helpDataRow.Description ?? helpDataRow.CategoryId ?? string.Empty);
                    break;
                case OptionsCategoryRow optionsCategoryRow:
                    label = HelpOptionsKeybindsLocalization.LocalizeOptionCategoryLabel(
                        optionsCategoryRow.CategoryId,
                        optionsCategoryRow.Title ?? optionsCategoryRow.CategoryId ?? string.Empty);
                    break;
                case KeybindCategoryRow keybindCategoryRow:
                    label = HelpOptionsKeybindsLocalization.LocalizeCommandCategoryLabel(
                        keybindCategoryRow.CategoryId,
                        keybindCategoryRow.CategoryDescription ?? keybindCategoryRow.CategoryId ?? string.Empty);
                    break;
            }

            if (!string.IsNullOrEmpty(label))
            {
                __instance.text.SetText($"{{{{C|{label}}}}}");
            }
        }
    }
}
