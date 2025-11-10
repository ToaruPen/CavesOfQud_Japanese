using HarmonyLib;
using QudJP.Localization;
using XRL;

namespace QudJP.Patches
{
    [HarmonyPatch(typeof(XmlDataHelper), nameof(XmlDataHelper.AssertExtraAttributes))]
    internal static class XmlDataHelperLegacyAttributePatch
    {
        private static void Prefix(XmlDataHelper __instance)
        {
            LocalizationAssetResolver.IgnoreLegacyAttributes(__instance);
        }
    }
}
