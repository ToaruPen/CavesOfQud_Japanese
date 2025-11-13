using HarmonyLib;
using ModelShark;

namespace QudJP.Patches
{
    /// <summary>
    /// Ensures every tooltip instance created via TooltipTrigger.Initialize immediately receives the JP font stack.
    /// TooltipManager reuses Tooltip objects, so we need to sanitize them when they are first constructed.
    /// </summary>
    [HarmonyPatch(typeof(TooltipTrigger))]
    internal static class TooltipFontEnforcerPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("Initialize")]
        private static void AfterInitialize(TooltipTrigger __instance)
        {
            if (__instance == null)
            {
                return;
            }

            // Guard the instantiated TooltipStyle clone that backs this trigger.
            TMPFontGuard.ApplyToHierarchy(__instance.Tooltip?.GameObject, forceReplace: true, includeInactive: true);

            // Also cover the trigger itself in case tooltipStyle is nested under it.
            TMPFontGuard.ApplyToHierarchy(__instance.tooltipStyle?.gameObject, forceReplace: true, includeInactive: true);
        }
    }
}
