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
            // Translate default serialized labels present on prefabs.
            var current = __instance.text;
            if (!string.IsNullOrEmpty(current))
            {
                var translated = Localization.Translator.Instance.Apply(current, __instance.GetType().FullName);
                if (!string.IsNullOrEmpty(translated) && !string.Equals(translated, current))
                {
                    __instance.text = translated;
                }
            }
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
            var current = __instance.text;
            if (!string.IsNullOrEmpty(current))
            {
                var translated = Localization.Translator.Instance.Apply(current, __instance.GetType().FullName);
                if (!string.IsNullOrEmpty(translated) && !string.Equals(translated, current))
                {
                    __instance.text = translated;
                }
            }
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
