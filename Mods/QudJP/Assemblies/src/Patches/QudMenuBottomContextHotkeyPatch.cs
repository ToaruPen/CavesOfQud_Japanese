using System;
using HarmonyLib;
using Qud.UI;
using QudJP.Localization;

namespace QudJP.Patches
{
    /// <summary>
    /// Normalizes hotkey labels for the HUD bottom context buttons so they always reflect the bound keys.
    /// </summary>
    [HarmonyPatch(typeof(QudMenuBottomContext))]
    internal static class QudMenuBottomContextHotkeyPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(QudMenuBottomContext.RefreshButtons))]
        private static void NormalizeItemHotkeys(QudMenuBottomContext __instance)
        {
            if (__instance == null)
            {
                return;
            }

            Normalize(__instance.items);
        }

        private static void Normalize(System.Collections.Generic.List<QudMenuItem>? items)
        {
            if (items == null || items.Count == 0)
            {
                return;
            }

            for (int i = 0; i < items.Count; i++)
            {
                var entry = items[i];
                var normalized = MenuItemTextLocalizer.Apply(entry.text, entry.command, entry.hotkey);
                if (string.Equals(normalized, entry.text, StringComparison.Ordinal))
                {
                    continue;
                }

                entry.text = normalized;
                items[i] = entry;
            }
        }
    }
}
