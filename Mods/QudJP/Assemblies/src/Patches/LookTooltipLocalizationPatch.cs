using System.Text;
using HarmonyLib;
using QudJP.Localization;
using XRL.UI;
using XRL.World;

namespace QudJP.Patches
{
    [HarmonyPatch(typeof(Look))]
    internal static class LookTooltipLocalizationPatch
    {
        private static bool _logged;
        [HarmonyPostfix]
        [HarmonyPatch(nameof(Look.GenerateTooltipInformation))]
        private static void LocalizeTooltip(ref Look.TooltipInformation __result)
        {
            if (!_logged)
            {
                _logged = true;
                UnityEngine.Debug.Log("[QudJP] LookTooltipLocalizationPatch active");
            }
            __result.DisplayName = Translator.Instance.Apply(__result.DisplayName, "Look.DisplayName");
            __result.LongDescription = TooltipTextLocalizer.ApplyLongDescription(__result.LongDescription);
            __result.FeelingText = TooltipTextLocalizer.ApplyFeeling(__result.FeelingText);
            __result.DifficultyText = TooltipTextLocalizer.ApplyDifficulty(__result.DifficultyText);
            __result.WoundLevel = TooltipTextLocalizer.ApplyWoundLevel(__result.WoundLevel);
        }
    }

    [HarmonyPatch(typeof(Look))]
    internal static class LookTooltipContentTranslationPatch
    {
        private const string BodyContextId = "XRL.UI.Look.GenerateTooltipContent.Body";

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Look.GenerateTooltipContent))]
        private static bool TranslateBody(GameObject O, ref string __result)
        {
            if (O == null)
            {
                __result = string.Empty;
                return false;
            }

            var info = Look.GenerateTooltipInformation(O);
            var builder = new StringBuilder(512);

            builder.AppendLine(info.DisplayName);
            builder.AppendLine();
            builder.AppendLine(info.LongDescription);
            builder.AppendLine();
            builder.AppendLine();
            builder.AppendLine(info.SubHeader);
            builder.AppendLine(info.WoundLevel);

            var body = builder.ToString();
            if (string.IsNullOrEmpty(body))
            {
                __result = string.Empty;
                return false;
            }

            var localized = Translator.Instance.Apply(body, BodyContextId);
            __result = string.IsNullOrEmpty(localized) ? body : localized;
            return false;
        }
    }
}
