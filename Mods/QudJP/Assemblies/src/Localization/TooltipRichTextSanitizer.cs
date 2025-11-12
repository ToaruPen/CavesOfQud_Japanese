using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace QudJP.Localization
{
    internal static class TooltipRichTextSanitizer
    {
        private static readonly Regex EmptyColorBlock = new("<color[^>]*>\\s*</color>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex ColorTag = new("<color(?<spec>[^>]*)>", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static string Sanitize(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var sanitized = EmptyColorBlock.Replace(value!, string.Empty);
            sanitized = ColorTag.Replace(sanitized, NormalizeColorTag);
            sanitized = RemoveDanglingClosures(sanitized);
            return sanitized;
        }

        private static string NormalizeColorTag(Match match)
        {
            var spec = match.Groups["spec"].Value;
            var normalized = NormalizeSpec(spec);
            return normalized.Length > 0 ? $"<color={normalized}>" : string.Empty;
        }

        private static string NormalizeSpec(string spec)
        {
            if (string.IsNullOrWhiteSpace(spec))
            {
                return string.Empty;
            }

            var trimmed = spec.Trim();
            if (trimmed.StartsWith("=", StringComparison.Ordinal))
            {
                trimmed = trimmed.Substring(1).Trim();
            }

            if (string.IsNullOrEmpty(trimmed))
            {
                return string.Empty;
            }

            if (trimmed[0] == '#')
            {
                var hex = trimmed.Substring(1);
                if (hex.Length != 6 && hex.Length != 8)
                {
                    return string.Empty;
                }

                for (int i = 0; i < hex.Length; i++)
                {
                    if (!Uri.IsHexDigit(hex[i]))
                    {
                        return string.Empty;
                    }
                }

                return $"#{hex}";
            }

            foreach (var c in trimmed)
            {
                if (!char.IsLetter(c))
                {
                    return string.Empty;
                }
            }

            return trimmed;
        }

        private static string RemoveDanglingClosures(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            var builder = new StringBuilder(value.Length);
            var openTags = new Stack<(int position, int length)>();

            for (int i = 0; i < value.Length;)
            {
                if (StartsWith(value, i, "<color", ignoreCase: true, out var openLength))
                {
                    var closing = value.IndexOf('>', i + openLength);
                    if (closing < 0)
                    {
                        break;
                    }

                    var tagLength = closing - i + 1;
                    openTags.Push((builder.Length, tagLength));
                    builder.Append(value, i, tagLength);
                    i += tagLength;
                    continue;
                }

                if (StartsWith(value, i, "</color>", ignoreCase: true, out _))
                {
                    if (openTags.Count > 0)
                    {
                        openTags.Pop();
                        builder.Append("</color>");
                    }
                    i += 8;
                    continue;
                }

                builder.Append(value[i]);
                i++;
            }

            while (openTags.Count > 0)
            {
                var (pos, len) = openTags.Pop();
                builder.Remove(pos, len);
            }

            return builder.ToString();
        }

        private static bool StartsWith(string value, int index, string token, bool ignoreCase, out int tokenLength)
        {
            tokenLength = token.Length;
            if (index + tokenLength > value.Length)
            {
                return false;
            }

            var comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            return string.Compare(value, index, token, 0, tokenLength, comparison) == 0;
        }
    }
}
