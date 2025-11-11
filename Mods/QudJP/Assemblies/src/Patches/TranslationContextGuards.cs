using System;

namespace QudJP.Patches
{
    internal static class TranslationContextGuards
    {
        public static bool ShouldSkipTranslation(string? contextId, string? eid, string? value)
        {
            if (string.IsNullOrEmpty(contextId) || string.IsNullOrEmpty(value))
            {
                return false;
            }

            var context = contextId!;

            if (context.StartsWith("ModelShark.Tooltip.", StringComparison.OrdinalIgnoreCase))
            {
                return TooltipParamMapCache.IsLocalizedValue(context, eid, value);
            }

            if (context.StartsWith("TMP.InventoryLine.", StringComparison.OrdinalIgnoreCase))
            {
                return InventoryParamMapCache.IsLocalizedValue(context, eid, value);
            }

            return false;
        }
    }
}
