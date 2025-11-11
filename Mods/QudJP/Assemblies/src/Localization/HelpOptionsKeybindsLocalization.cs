using System;
using System.Collections.Generic;
using System.Linq;

namespace QudJP.Localization
{
    internal static class HelpOptionsKeybindsLocalization
    {
        internal static string LocalizeHelpTitle(string categoryId, string fallback)
        {
            return SafeStringTranslator.SafeTranslate(
                fallback ?? string.Empty,
                BuildContext("Help.Topic", categoryId, "Title"));
        }

        internal static string LocalizeHelpBody(string categoryId, string fallback)
        {
            return SafeStringTranslator.SafeTranslate(
                fallback ?? string.Empty,
                BuildContext("Help.Topic", categoryId, "Body"));
        }

        internal static string LocalizeHelpCategoryLabel(string categoryId, string fallback)
        {
            return SafeStringTranslator.SafeTranslate(
                fallback ?? string.Empty,
                BuildContext("Help.Category", categoryId, "Label"));
        }

        internal static string LocalizeOptionTitle(string optionId, string fallback)
        {
            return SafeStringTranslator.SafeTranslate(
                fallback ?? string.Empty,
                BuildContext("Options.Option", optionId, "Title"));
        }

        internal static string LocalizeOptionHelp(string optionId, string fallback)
        {
            return SafeStringTranslator.SafeTranslate(
                fallback ?? string.Empty,
                BuildContext("Options.Option", optionId, "Help"));
        }

        internal static string LocalizeOptionCategoryLabel(string categoryId, string fallback)
        {
            return SafeStringTranslator.SafeTranslate(
                fallback ?? string.Empty,
                BuildContext("Options.Category", categoryId, "Label"));
        }

        internal static string LocalizeOptionValue(string optionId, string fallback, string valueKey)
        {
            return SafeStringTranslator.SafeTranslate(
                fallback ?? string.Empty,
                BuildContext("Options.Option", optionId, $"Value.{valueKey}"));
        }

        internal static string BuildOptionSearchKeywords(IEnumerable<string?> parts)
        {
            return string.Join(
                " ",
                parts.Where(p => !string.IsNullOrWhiteSpace(p))
                     .Select(p => p!.Trim()));
        }

        internal static string LocalizeCommandDisplay(string commandId, string fallback)
        {
            return SafeStringTranslator.SafeTranslate(
                fallback ?? string.Empty,
                BuildContext("CommandBinding", commandId, "Display"));
        }

        internal static string LocalizeCommandCategoryLabel(string categoryId, string fallback)
        {
            return SafeStringTranslator.SafeTranslate(
                fallback ?? string.Empty,
                BuildContext("CommandBinding.Category", categoryId, "Label"));
        }

        internal static string LocalizeKeybindNone(string fallback)
        {
            return SafeStringTranslator.SafeTranslate(
                fallback ?? string.Empty,
                "Qud.UI.KeybindRow.BindBox.None");
        }

        internal static string LocalizeKeybindInputBanner(string deviceLabel)
        {
            var template = SafeStringTranslator.SafeTranslate(
                "{{C|Configuring Controller:}} {{c|{device}}}",
                "Qud.UI.KeybindsScreen.InputType");
            return template.Replace("{device}", deviceLabel ?? string.Empty);
        }

        internal static string LocalizeControlTypeLabel(string deviceKey, string fallback)
        {
            return SafeStringTranslator.SafeTranslate(
                fallback ?? string.Empty,
                BuildContext("Keybinds.ControlType", deviceKey, "Label"));
        }

        internal static string LocalizeNoControllerDetected(string fallback)
        {
            return SafeStringTranslator.SafeTranslate(
                fallback ?? string.Empty,
                "Keybinds.ControlType.NoController");
        }

        internal static string LocalizeSearchPlaceholder(string fallback)
        {
            return SafeStringTranslator.SafeTranslate(
                fallback ?? string.Empty,
                "FrameworkSearchInput.Placeholder");
        }

        internal static string LocalizeSearchPopupTitle(string fallback)
        {
            return SafeStringTranslator.SafeTranslate(
                fallback ?? string.Empty,
                "FrameworkSearchInput.PopupTitle");
        }

        private static string BuildContext(string prefix, string? token, string? role)
        {
            var normalized = string.IsNullOrWhiteSpace(token) ? "Default" : token!.Trim();
            return string.IsNullOrWhiteSpace(role)
                ? $"{prefix}.{normalized}"
                : $"{prefix}.{normalized}.{role}";
        }
    }
}
