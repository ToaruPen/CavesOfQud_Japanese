using HarmonyLib;
using Qud.UI;

namespace QudJP.Patches
{
    [HarmonyPatch(typeof(PopupMessage))]
    internal static class PopupMessageFontGuardPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(PopupMessage.ShowPopup))]
        private static void AfterShowPopup(PopupMessage __instance)
        {
            TMPFontGuard.ApplyToHierarchy(__instance, forceReplace: true, includeInactive: true);
        }
    }
}
