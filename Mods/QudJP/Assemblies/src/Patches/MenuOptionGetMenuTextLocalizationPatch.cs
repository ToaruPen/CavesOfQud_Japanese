using HarmonyLib;
using QudJP.Localization;
using XRL.UI.Framework;

namespace QudJP
{
    /// <summary>
    /// メニューの描画直前にも Description を日本語化し、他のスケーラでも確実に反映する。
    /// </summary>
    [HarmonyPatch(typeof(MenuOption), nameof(MenuOption.getMenuText))]
    internal static class MenuOptionGetMenuTextLocalizationPatch
    {
        private static void Prefix(MenuOption __instance)
        {
            MenuOptionLegendLocalizer.TryApply(__instance);
        }
    }
}
