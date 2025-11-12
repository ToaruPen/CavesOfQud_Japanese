using HarmonyLib;
using ModelShark;
using QudJP.Diagnostics;
using QudJP.Localization;

namespace QudJP.Patches
{
    [HarmonyPatch(typeof(TooltipTrigger))]
    internal static class TooltipTriggerSetTextPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(TooltipTrigger.SetText))]
        private static void BeforeSetText(TooltipTrigger __instance, string parameterName, ref string text)
        {
            var styleName = ResolveStyleName(__instance);
            var processed = TooltipFieldLocalizer.Process(styleName, parameterName, text);

            if (TooltipFieldLocalizer.IsSubHeaderField(parameterName) && string.IsNullOrWhiteSpace(processed))
            {
                TooltipSourceContext.TryGetSubjects(__instance, out var primary, out var compare);
                var subject = TooltipFieldLocalizer.TargetsSecondarySubject(parameterName) ? compare : primary;
                processed = TooltipSubHeaderBuilder.Build(subject) ?? string.Empty;
            }

            text = string.IsNullOrWhiteSpace(processed) ? string.Empty : processed!;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(TooltipTrigger.SetText))]
        private static void AfterSetText(TooltipTrigger __instance, string parameterName, string text) =>
            EnsureParameterizedField(__instance, parameterName, text);

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

        private static string? ResolveStyleName(TooltipTrigger trigger)
        {
            if (trigger == null)
            {
                return null;
            }

            var style = TooltipTraversal.ResolveStyleName(trigger.Tooltip);
            if (!string.IsNullOrEmpty(style))
            {
                return style;
            }

            return trigger.tooltipStyle != null ? trigger.tooltipStyle.name : null;
        }
    }
}
