using HarmonyLib;
using Qud.UI;

namespace QudJP.Patches
{
    [HarmonyPatch(typeof(BaseLineWithTooltip))]
    internal static class BaseLineWithTooltipFontGuardPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(BaseLineWithTooltip.StartTooltip))]
        private static void AfterStartTooltip(BaseLineWithTooltip __instance)
        {
            var trigger = __instance?.tooltip;
            if (trigger == null)
            {
                return;
            }

            TMPFontGuard.ApplyToHierarchy(trigger, forceReplace: true, includeInactive: true);
            TMPFontGuard.ApplyToHierarchy(trigger.Tooltip?.GameObject, forceReplace: true, includeInactive: true);
        }
    }
}
