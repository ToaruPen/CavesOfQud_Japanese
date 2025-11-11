using HarmonyLib;
using Qud.UI;
using QudJP.Diagnostics;
using XRL.World;

namespace QudJP.Patches
{
    /// <summary>
    /// Records which UI element spawned a tooltip so later logging can mention the source item.
    /// </summary>
    [HarmonyPatch(typeof(BaseLineWithTooltip))]
    internal static class BaseLineTooltipContextPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(BaseLineWithTooltip.StartTooltip))]
        private static void AfterStartTooltip(
            BaseLineWithTooltip __instance,
            GameObject go,
            GameObject compareGo)
        {
            var trigger = __instance?.tooltip;
            if (trigger == null)
            {
                return;
            }

            TooltipSourceContext.Record(trigger, __instance, go, compareGo);
        }
    }
}
