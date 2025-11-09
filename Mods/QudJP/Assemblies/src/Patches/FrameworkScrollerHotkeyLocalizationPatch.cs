using HarmonyLib;
using System.Collections.Generic;
using XRL.CharacterBuilds;
using XRL.UI;
using XRL.UI.Framework;
using QudJP.Localization;

namespace QudJP
{
    /// <summary>
    /// FrameworkScroller がメニュー／ホットキーリストを描画する直前に Description を日本語へ差し替える。
    /// これによりキャラ作成など他画面でも "navigate / select / quit" のような英語テキストを統一的に翻訳できる。
    /// </summary>
    [HarmonyPatch(
        typeof(FrameworkScroller),
        nameof(FrameworkScroller.BeforeShow),
        typeof(EmbarkBuilderModuleWindowDescriptor),
        typeof(IEnumerable<FrameworkDataElement>))]
    internal static class FrameworkScrollerHotkeyLocalizationPatch
    {
        private static void Prefix(
            FrameworkScroller __instance,
            EmbarkBuilderModuleWindowDescriptor descriptor,
            IEnumerable<FrameworkDataElement> selections)
        {
            if (__instance == null)
            {
                return;
            }

            if (selections != null)
            {
                Localize(selections);
            }
            else if (__instance.choices != null)
            {
                Localize(__instance.choices);
            }
        }

        private static void Localize(IEnumerable<FrameworkDataElement> entries)
        {
            foreach (var entry in entries)
            {
                switch (entry)
                {
                    case MenuOption option:
                        TryLocalizedDescription(option);
                        break;
                    case IFrameworkDataList list:
                        var children = list.getChildren();
                        if (children != null)
                        {
                            Localize(children);
                        }
                        break;
                }
            }
        }

        private static void TryLocalizedDescription(MenuOption option)
        {
            if (option == null)
            {
                return;
            }

            MenuOptionLegendLocalizer.TryApply(option);
        }
    }
}
