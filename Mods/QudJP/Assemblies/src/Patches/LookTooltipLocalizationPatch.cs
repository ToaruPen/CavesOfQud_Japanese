using HarmonyLib;
using QudJP.Localization;
using XRL.UI;

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
}
