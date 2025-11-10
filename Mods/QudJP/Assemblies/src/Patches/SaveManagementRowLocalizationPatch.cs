using HarmonyLib;
using QudJP.Localization;
using XRL.UI.Framework;

namespace QudJP.Patches
{
    /// <summary>
    /// Replaces SaveManagement row labels with Translator-backed strings so the dialog shows Japanese captions.
    /// </summary>
    [HarmonyPatch(typeof(SaveManagementRow))]
    internal static class SaveManagementRowLocalizationPatch
    {
        private const string LocationContext = "SaveManagement.Location";
        private const string LastSavedContext = "SaveManagement.LastSaved";

        [HarmonyPostfix]
        [HarmonyPatch(nameof(SaveManagementRow.setData))]
        private static void LocalizeRow(
            SaveManagementRow __instance,
            FrameworkDataElement data)
        {
            if (__instance?.TextSkins == null ||
                data is not SaveInfoData saveInfo ||
                saveInfo.SaveGame == null)
            {
                return;
            }

            var save = saveInfo.SaveGame;

            if (__instance.TextSkins.Count > 1)
            {
                var label = Translator.Instance.Apply("Location:", LocationContext);
                __instance.TextSkins[1].SetText($"{Colorize("C", label)} {save.Info}");
            }

            if (__instance.TextSkins.Count > 2)
            {
                var label = Translator.Instance.Apply("Last saved:", LastSavedContext);
                __instance.TextSkins[2].SetText($"{Colorize("C", label)} {save.SaveTime}");
            }

            // TextSkins[3] only shows size and internal ID, so we keep original formatting.
        }

        private static string Colorize(string color, string text) => $"{{{{{color}|{text}}}}}";
    }
}
