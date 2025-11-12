using System;
using System.Text;

namespace QudJP.Localization
{
    internal static class TooltipDisplayNameLocalizer
    {
        private static readonly char[] StatGlyphSentinels = { '\u0004', '\u0003', '\u001a', '\t' };

        public static string Localize(string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            var prefix = ExtractLeadingIconBlock(value!, out var remainder);
            var localized = Translator.Instance.Apply(remainder, "Look.DisplayName");
            var baseText = string.IsNullOrEmpty(localized) ? remainder : localized;

            if (string.IsNullOrEmpty(prefix))
            {
                return baseText;
            }

            return prefix + baseText;
        }

        private static string ExtractLeadingIconBlock(string value, out string remainder)
        {
            var sb = new StringBuilder(value.Length);
            int index = 0;
            int lastIconEnd = 0;

            while (index < value.Length - 3 && value[index] == '{' && value[index + 1] == '{')
            {
                var close = value.IndexOf("}}", index + 2, StringComparison.Ordinal);
                if (close < 0)
                {
                    break;
                }

                var segment = value.Substring(index, close - index + 2);
                if (!ContainsStatGlyph(segment))
                {
                    break;
                }

                sb.Append(segment);
                index = close + 2;
                lastIconEnd = index;

                while (index < value.Length && char.IsWhiteSpace(value[index]))
                {
                    sb.Append(value[index]);
                    index++;
                    lastIconEnd = index;
                }
            }

            if (lastIconEnd == 0)
            {
                remainder = value;
                return string.Empty;
            }

            remainder = value.Substring(lastIconEnd);
            return sb.ToString();
        }

        private static bool ContainsStatGlyph(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            for (int i = 0; i < value.Length; i++)
            {
                if (Array.IndexOf(StatGlyphSentinels, value[i]) >= 0)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
