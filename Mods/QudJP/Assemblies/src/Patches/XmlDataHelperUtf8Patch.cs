using HarmonyLib;
using QudJP.Localization;
using XRL;

namespace QudJP.Patches
{
    [HarmonyPatch(typeof(XmlDataHelper), nameof(XmlDataHelper.Read))]
    internal static class XmlDataHelperUtf8Patch
    {
        private static void Postfix(XmlDataHelper __instance)
        {
            LocalizationAssetResolver.EnsureUtf8Passthrough(__instance);
        }
    }
}
