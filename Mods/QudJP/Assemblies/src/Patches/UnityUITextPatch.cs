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
            FontManager.Instance.ApplyToLegacyText(__instance);
        }
    }
}
