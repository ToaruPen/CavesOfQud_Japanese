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
            // 初期文字列で日本語が含まれている場合のみフォント適用（英語UIのレイアウトを保つ）
            FontManager.Instance.ApplyToLegacyText(__instance);
            var current = __instance.text;
            if (!string.IsNullOrEmpty(current))
            {
                var translated = Translator.Instance.Apply(current, __instance.GetType().FullName);
                if (!string.IsNullOrEmpty(translated) && !string.Equals(translated, current))
                {
                    __instance.text = translated;
                    // 翻訳後に日本語が含まれる可能性があるので再適用
                    FontManager.Instance.ApplyToLegacyText(__instance);
                }
            }
        }
    }
}
