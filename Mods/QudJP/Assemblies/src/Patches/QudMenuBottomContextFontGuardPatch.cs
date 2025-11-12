using HarmonyLib;
using Qud.UI;

namespace QudJP.Patches
{
    [HarmonyPatch(typeof(QudMenuBottomContext))]
    internal static class QudMenuBottomContextFontGuardPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(QudMenuBottomContext.RefreshButtons))]
        private static void AfterRefresh(QudMenuBottomContext __instance)
        {
            if (__instance?.buttons == null)
            {
                return;
            }

            foreach (var button in __instance.buttons)
            {
                TMPFontGuard.ApplyToHierarchy(button, forceReplace: true, includeInactive: true);
            }
        }
    }
}
