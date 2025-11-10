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

        internal static string Apply(string? text, string? contextSuffix)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text ?? string.Empty;
            }

            var context = string.IsNullOrWhiteSpace(contextSuffix)
                ? "QudMenuItem"
                : $"QudMenuItem.{contextSuffix}";

            var replaced = ColorTagRegex.Replace(
                text,
                match =>
                {
                    var body = match.Groups["body"].Value;
                    if (!ShouldTranslate(body))
                    {
                        return match.Value;
                    }

                    var localized = Translator.Instance.Apply(body, context);
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

            var fallback = Translator.Instance.Apply(text, context) ?? string.Empty;
            return string.IsNullOrEmpty(fallback) ? (text ?? string.Empty) : fallback;
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
    }
}
