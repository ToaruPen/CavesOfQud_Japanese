using System;
using System.Text.RegularExpressions;

namespace QudJP.Localization
{
    /// <summary>
    /// Applies Translator rules to the label segments inside QudMenuItem text while preserving color tags.
    /// </summary>
    internal static class MenuItemTextLocalizer
    {
        private static readonly Regex ColorTagRegex = new(
            "\\{\\{(?<color>[^|{}]+)\\|(?<body>.*?)\\}\\}",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        internal static string Apply(string? text, string? command, string? hotkey)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text ?? string.Empty;
            }

            var suffix = BuildContextSuffix(command, hotkey);
            var context = string.IsNullOrWhiteSpace(suffix)
                ? "QudMenuItem"
                : $"QudMenuItem.{suffix}";

            var replaced = ColorTagRegex.Replace(
                text,
                match =>
                {
                    var body = match.Groups["body"].Value;
                    var localized = ShouldTranslate(body)
                        ? SafeStringTranslator.SafeTranslate(body, context)
                        : body;
                    localized = NormalizeHotkeyIfNeeded(command, hotkey, match.Groups["color"].Value, localized);
                    if (string.IsNullOrEmpty(localized) ||
                        string.Equals(localized, body, StringComparison.Ordinal))
                    {
                        return match.Value;
                    }

                    return $"{{{{{match.Groups["color"].Value}|{localized}}}}}";
                });

            if (!string.Equals(replaced, text, StringComparison.Ordinal))
            {
                return replaced;
            }

            var fallback = SafeStringTranslator.SafeTranslate(text, context);
            if (!string.IsNullOrEmpty(fallback) &&
                !string.Equals(fallback, text, StringComparison.Ordinal))
            {
                return fallback;
            }

            var shared = SafeStringTranslator.SafeTranslate(text, "QudMenuItem");
            return string.IsNullOrEmpty(shared) ? (text ?? string.Empty) : shared;
        }

        private static bool ShouldTranslate(string body)
        {
            if (string.IsNullOrWhiteSpace(body))
            {
                return false;
            }

            var trimmed = body.Trim();
            if (trimmed.Length == 0)
            {
                return false;
            }

            if (trimmed.StartsWith("[", StringComparison.Ordinal) &&
                trimmed.EndsWith("]", StringComparison.Ordinal))
            {
                return false;
            }

            return true;
        }

        private static string NormalizeHotkeyIfNeeded(string? command, string? hotkey, string? colorTag, string? value)
        {
            if (!string.Equals(colorTag, "W", StringComparison.OrdinalIgnoreCase))
            {
                return value ?? string.Empty;
            }

            var baseValue = value ?? string.Empty;
            var trimmed = baseValue.Trim();
            if (trimmed.Length == 0)
            {
                return baseValue;
            }

            var label = MenuHotkeyHelper.GetPrimaryLabel(command, hotkey);
            if (!string.IsNullOrWhiteSpace(label))
            {
                var normalized = $"[{label!.Trim()}]";
                return string.Equals(baseValue, trimmed, StringComparison.Ordinal)
                    ? normalized
                    : baseValue.Replace(trimmed, normalized);
            }

            if (trimmed.StartsWith("[", StringComparison.Ordinal) &&
                trimmed.EndsWith("]", StringComparison.Ordinal))
            {
                return baseValue;
            }

            var fallback = $"[{trimmed}]";
            return string.Equals(baseValue, trimmed, StringComparison.Ordinal)
                ? fallback
                : baseValue.Replace(trimmed, fallback);
        }

        private static string BuildContextSuffix(string? command, string? hotkey)
        {
            if (!string.IsNullOrWhiteSpace(command))
            {
                return command!;
            }

            if (!string.IsNullOrWhiteSpace(hotkey))
            {
                return hotkey!;
            }

            return "Label";
        }

    }
}
