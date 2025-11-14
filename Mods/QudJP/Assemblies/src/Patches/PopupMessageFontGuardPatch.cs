using HarmonyLib;
using Qud.UI;

namespace QudJP.Patches
{
    [HarmonyPatch(typeof(PopupMessage))]
    internal static class PopupMessageFontGuardPatch
    {
        /// <summary>
        /// Pre-flight guard so the popup starts with JP fonts before any text layout happens.
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(nameof(PopupMessage.ShowPopup))]
        [HarmonyPriority(Priority.First)]
        private static void BeforeShowPopup(PopupMessage __instance)
        {
            TMPFontGuard.ApplyToHierarchy(__instance, forceReplace: true, includeInactive: true);
            QudJP.Diagnostics.JpLog.Info(QudJP.Diagnostics.UIContext.Current, "FontGuard", "Popup/BEFORE", $"root='{__instance?.gameObject?.name ?? "<null>"}'");
        }

        /// <summary>
        /// Post guard retains compatibility with any late-created children (e.g. buttons spawned during ShowPopup).
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(nameof(PopupMessage.ShowPopup))]
        private static void AfterShowPopup(PopupMessage __instance)
        {
            TMPFontGuard.ApplyToHierarchy(__instance, forceReplace: true, includeInactive: true);
            QudJP.Diagnostics.JpLog.Info(QudJP.Diagnostics.UIContext.Current, "FontGuard", "Popup/AFTER", $"root='{__instance?.gameObject?.name ?? "<null>"}'");
        }
    }
}
