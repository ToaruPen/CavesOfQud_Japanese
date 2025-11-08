using HarmonyLib;
using TMPro;

namespace QudJP
{
    [HarmonyPatch(typeof(TextMeshProUGUI))]
    internal static class TextMeshProUguiPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("OnEnable")]
        private static void OnEnable(TextMeshProUGUI __instance)
        {
            FontManager.Instance.ApplyToText(__instance);
        }
    }

    [HarmonyPatch(typeof(TextMeshPro))]
    internal static class TextMeshProWorldPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("OnEnable")]
        private static void OnEnable(TextMeshPro __instance)
        {
            FontManager.Instance.ApplyToText(__instance);
        }
    }

    [HarmonyPatch(typeof(TMP_InputField))]
    internal static class TMPInputFieldPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("OnEnable")]
        private static void OnEnable(TMP_InputField __instance)
        {
            FontManager.Instance.ApplyToInputField(__instance);
        }
    }
}
