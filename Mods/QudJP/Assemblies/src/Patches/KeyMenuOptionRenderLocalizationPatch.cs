using HarmonyLib;
using QudJP.Localization;
using XRL.UI.Framework;

namespace QudJP
{
    /// <summary>
    /// legend バーで描画されるテキストにも日本語訳を適用する。
    /// </summary>
    [HarmonyPatch(typeof(KeyMenuOption))]
    internal static class KeyMenuOptionRenderLocalizationPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(KeyMenuOption.Render))]
        private static void Prefix(ref string text)
        {
            if (MenuOptionLegendLocalizer.TryLocalizeLiteral(text, out var localized))
            {
                text = localized!;
            }
        }
    }
}
