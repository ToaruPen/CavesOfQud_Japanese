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
        [HarmonyPatch(nameof(TooltipTrigger.SetText), new[] { typeof(string), typeof(string) })]
        private static void BeforeSetTextWithField(TooltipTrigger __instance, string parameterName, ref string text) =>
            ProcessText(__instance, parameterName, ref text);

        [HarmonyPrefix]
        [HarmonyPatch(nameof(TooltipTrigger.SetText), new[] { typeof(string) })]
        private static void BeforeSetTextBodyOnly(TooltipTrigger __instance, ref string text) =>
            ProcessText(__instance, null, ref text);

        [HarmonyPostfix]
        [HarmonyPatch(nameof(TooltipTrigger.SetText), new[] { typeof(string), typeof(string) })]
        private static void AfterSetText(TooltipTrigger __instance, string parameterName, string text) =>
            EnsureParameterizedField(__instance, parameterName, text);

        private static void ProcessText(TooltipTrigger trigger, string? parameterName, ref string text)
        {
            if (trigger == null || string.IsNullOrEmpty(text))
            {
                return;
            }

            var styleName = ResolveStyleName(trigger);
            var partition = TooltipIconPartitioner.Partition(text);
            var localized = TooltipFieldLocalizer.Process(styleName, parameterName, partition.Label);
            var processed = partition.HasIcons
                ? string.Concat(partition.Prefix, localized, partition.Suffix)
                : localized;

            if (TooltipFieldLocalizer.IsSubHeaderField(parameterName) && string.IsNullOrWhiteSpace(processed))
            {
                TooltipSourceContext.TryGetSubjects(trigger, out var primary, out var compare);
                var subject = TooltipFieldLocalizer.TargetsSecondarySubject(parameterName) ? compare : primary;
                processed = TooltipSubHeaderBuilder.Build(subject) ?? string.Empty;
            }

            text = string.IsNullOrWhiteSpace(processed) ? string.Empty : processed!;
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
