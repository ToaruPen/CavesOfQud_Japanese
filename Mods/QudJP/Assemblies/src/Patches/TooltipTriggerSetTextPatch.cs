using HarmonyLib;
using ModelShark;

namespace QudJP.Patches
{
    [HarmonyPatch(typeof(TooltipTrigger))]
    internal static class TooltipTriggerSetTextPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(TooltipTrigger.SetText))]
        private static void AfterSetText(TooltipTrigger __instance, string parameterName, string text)
        {
            TooltipBodyTextCache.Remember(__instance, parameterName, text);
            EnsureParameterizedField(__instance, parameterName, text);
        }

        private static void EnsureParameterizedField(TooltipTrigger trigger, string parameterName, string value)
        {
            if (trigger == null || string.IsNullOrEmpty(parameterName))
            {
                return;
            }

            var fields = trigger.parameterizedTextFields;
            if (fields == null)
            {
                return;
            }

            for (int i = 0; i < fields.Count; i++)
            {
                var field = fields[i];
                if (field == null)
                {
                    continue;
                }

                if (!string.Equals(field.name, parameterName, System.StringComparison.Ordinal))
                {
                    continue;
                }

                if (string.IsNullOrEmpty(field.value) && !string.IsNullOrEmpty(value))
                {
                    field.value = value;
                }

                return;
            }

            var delimiter = TooltipManager.Instance != null
                ? TooltipManager.Instance.textFieldDelimiter
                : "%";

            fields.Add(new ParameterizedTextField
            {
                name = parameterName,
                placeholder = string.Concat(delimiter, parameterName, delimiter),
                value = value ?? string.Empty,
            });
        }
    }
}
