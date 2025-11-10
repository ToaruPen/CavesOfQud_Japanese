using System;
using HarmonyLib;
using Qud.UI;
using QudJP.Localization;

namespace QudJP.Patches
{
    /// <summary>
    /// Localizes QudMenuItem text before the UI composes highlighted strings.
    /// </summary>
    [HarmonyPatch(typeof(SelectableTextMenuItem))]
    internal static class SelectableTextMenuItemLocalizationPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(SelectableTextMenuItem.SelectChanged))]
        private static void LocalizeMenuItemText(SelectableTextMenuItem __instance)
        {
            if (__instance?.data is not QudMenuItem item)
            {
                return;
            }

            var translated = MenuItemTextLocalizer.Apply(item.text, BuildContext(item));
            if (string.Equals(translated, item.text, StringComparison.Ordinal))
            {
                return;
            }

            item.text = translated;
            __instance.data = item;
        }

        private static string BuildContext(in QudMenuItem item)
        {
            if (!string.IsNullOrWhiteSpace(item.command))
            {
                return item.command!;
            }

            if (!string.IsNullOrWhiteSpace(item.hotkey))
            {
                return item.hotkey!;
            }

            return "Label";
        }
    }
}
