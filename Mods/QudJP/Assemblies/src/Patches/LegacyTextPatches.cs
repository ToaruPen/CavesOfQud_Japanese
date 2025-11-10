using HarmonyLib;
using QudJP.Localization;
using UnityEngine.UI;

namespace QudJP.Patches
{
    [HarmonyPatch(typeof(Text))]
    internal static class LegacyTextPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("OnEnable")]
        private static void OnEnable(Text __instance)
        {
            FontManager.Instance.ApplyToLegacyText(__instance);
            var current = __instance.text;
            if (!string.IsNullOrEmpty(current))
            {
                var translated = Translator.Instance.Apply(current, __instance.GetType().FullName);
                if (!string.IsNullOrEmpty(translated) && !string.Equals(translated, current))
                {
                    __instance.text = translated;
                }
            }
        }
    }
}

