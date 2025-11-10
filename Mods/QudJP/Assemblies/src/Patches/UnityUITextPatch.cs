using HarmonyLib;
using UnityEngine.UI;

namespace QudJP
{
    [HarmonyPatch(typeof(Text))]
    internal static class UnityUITextPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("OnEnable")]
        private static void Apply(Text __instance)
        {
            // フォント置換は日本語が必要な場合のみ（英語UIのレイアウトを保持）
            FontManager.Instance.ApplyToLegacyText(__instance);
        }
    }
}
