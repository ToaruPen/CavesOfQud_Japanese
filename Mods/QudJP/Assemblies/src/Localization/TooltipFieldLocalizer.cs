using System;
using System.Text.RegularExpressions;

namespace QudJP.Localization
{
    internal static class TooltipFieldLocalizer
    {
        private static readonly Regex FreezingRegex = new("\\bfreezing\\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex VeryLowRegex = new("\\bVery\\s+Low\\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static string Process(string? styleName, string? parameterName, string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            var source = value!;
            if (LooksLikeRtf(source))
            {
                return source;
            }

            var normalized = NormalizeEmbeddedTokens(source);
            var kind = Classify(parameterName);

            switch (kind)
            {
                case TooltipFieldKind.LongDescription:
                    normalized = TooltipTextLocalizer.ApplyLongDescription(normalized);
                    break;
                case TooltipFieldKind.SubHeader:
                    normalized = TooltipTextLocalizer.ApplySubHeader(normalized);
                    break;
                case TooltipFieldKind.WoundLevel:
                    normalized = TooltipTextLocalizer.ApplyWoundLevel(normalized) ?? normalized;
                    break;
                case TooltipFieldKind.Feeling:
                    normalized = TooltipTextLocalizer.ApplyFeeling(normalized) ?? normalized;
                    break;
                case TooltipFieldKind.Difficulty:
                    normalized = TooltipTextLocalizer.ApplyDifficulty(normalized) ?? normalized;
                    break;
            }

            var contextId = BuildContext(styleName, parameterName);
            var translated = TooltipTokenizedTranslator.Translate(normalized, contextId);
            translated = TooltipRichTextSanitizer.Sanitize(translated);

            if (string.IsNullOrWhiteSpace(translated))
            {
                return source;
            }

            return translated;
        }

        public static bool LooksLikeRtf(string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            var text = value!;
            if (text.StartsWith("{\\rtf", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (text.IndexOf('{') >= 0 && text.IndexOf('}') > text.IndexOf('{') && text.IndexOf('\\') >= 0)
            {
                if (text.IndexOf("\\fonttbl", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    text.IndexOf("\\colortbl", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    text.IndexOf("\\stylesheet", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    text.IndexOf("\\pard", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }


        public static bool IsSubHeaderField(string? name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            var value = name!;
            return value.Equals("ConText", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("ConText2", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("SubHeader", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("SubHeader2", StringComparison.OrdinalIgnoreCase);
        }

        public static bool TargetsSecondarySubject(string? name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            return name!.EndsWith("2", StringComparison.Ordinal);
        }

        private static TooltipFieldKind Classify(string? parameterName)
        {
            if (string.IsNullOrEmpty(parameterName))
            {
                return TooltipFieldKind.Generic;
            }

            var lower = parameterName!.ToLowerInvariant();
            if (lower.Contains("long") || lower.Contains("desc"))
            {
                return TooltipFieldKind.LongDescription;
            }

            if (lower.Contains("context") || lower.Contains("subheader"))
            {
                return TooltipFieldKind.SubHeader;
            }

            if (lower.Contains("wound"))
            {
                return TooltipFieldKind.WoundLevel;
            }

            if (lower.Contains("feeling"))
            {
                return TooltipFieldKind.Feeling;
            }

            if (lower.Contains("difficulty"))
            {
                return TooltipFieldKind.Difficulty;
            }

            return TooltipFieldKind.Generic;
        }

        private static string NormalizeEmbeddedTokens(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            var result = value;
            if (result.IndexOf("(unburnt)", StringComparison.Ordinal) >= 0)
            {
                result = result.Replace("(unburnt)", "・未点火・");
            }

            result = FreezingRegex.Replace(result, "凍結");
            result = VeryLowRegex.Replace(result, "非常に低い");
            return result;
        }

        private static string BuildContext(string? styleName, string? parameterName)
        {
            var style = string.IsNullOrEmpty(styleName) ? "<null>" : styleName;
            var field = string.IsNullOrEmpty(parameterName) ? "Field" : parameterName;
            return $"ModelShark.Tooltip.{style}.{field}";
        }

        private enum TooltipFieldKind
        {
            Generic,
            LongDescription,
            SubHeader,
            WoundLevel,
            Feeling,
            Difficulty,
        }
    }
}
