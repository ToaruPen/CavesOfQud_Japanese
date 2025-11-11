using System;
using HarmonyLib;
using Qud.UI;
using QudJP.Localization;
using QudJP.Diagnostics;

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

            var eid = UIContext.Current ?? UIContext.Capture();
            UIContext.Bind(__instance, eid);
            JpLog.Info(eid, "Menu", "SelectChanged/IN", $"cmd={item.command ?? "<null>"} len={item.text?.Length ?? 0}");

            var translated = MenuItemTextLocalizer.Apply(item.text, item.command, item.hotkey);
            if (string.Equals(translated, item.text, StringComparison.Ordinal))
            {
                JpLog.Info(eid, "Menu", "SelectChanged/OUT", "unchanged");
                UIContext.Release(eid);
                return;
            }

            item.text = translated;
            __instance.data = item;

            JpLog.Info(eid, "Menu", "SelectChanged/OUT", $"len={translated?.Length ?? 0}");
            UIContext.Release(eid);
        }

    }
}
