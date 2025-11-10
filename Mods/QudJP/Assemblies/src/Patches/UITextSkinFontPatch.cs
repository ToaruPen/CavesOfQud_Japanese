using HarmonyLib;
using TMPro;
using XRL.UI;

namespace QudJP
{
    /// <summary>
    /// Ensures legacy UITextSkin instances also receive the JP font even if they were enabled before our OnEnable patches ran.
    /// </summary>
    [HarmonyPatch(typeof(UITextSkin))]
    internal static class UITextSkinFontPatch
    {
        private static readonly AccessTools.FieldRef<UITextSkin, TextMeshProUGUI?> TmpField =
            AccessTools.FieldRefAccess<UITextSkin, TextMeshProUGUI?>("_tmp");

        [HarmonyPostfix]
        [HarmonyPatch("Apply")]
        private static void ApplyFont(UITextSkin __instance)
        {
            var tmp = TmpField(__instance) ?? __instance.GetComponent<TextMeshProUGUI>();
            FontManager.Instance.ApplyToText(tmp);
        }
    }
}
