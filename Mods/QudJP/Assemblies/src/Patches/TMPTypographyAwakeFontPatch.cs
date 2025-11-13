using HarmonyLib;
using TMPro;

namespace QudJP.Patches
{
    /// <summary>
    /// Forces TMP UI texts to adopt the JP font the moment they awaken, before pooling kicks in.
    /// </summary>
    [HarmonyPatch(typeof(TextMeshProUGUI))]
    internal static class TextMeshProUguiAwakeFontPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("Awake")]
        private static void AfterAwake(TextMeshProUGUI __instance)
        {
            if (__instance == null)
            {
                return;
            }

            FontManager.Instance.ApplyToText(__instance, forceReplace: true);
        }
    }

    /// <summary>
    /// Same guard for world-space TMP texts (e.g., floating labels).
    /// </summary>
    [HarmonyPatch(typeof(TextMeshPro))]
    internal static class TextMeshProWorldAwakeFontPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("Awake")]
        private static void AfterAwake(TextMeshPro __instance)
        {
            if (__instance == null)
            {
                return;
            }

            FontManager.Instance.ApplyToText(__instance, forceReplace: true);
        }
    }
}
