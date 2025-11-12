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

            var text = value!;
            var builder = new StringBuilder(text.Length + 16);
            var buffer = new StringBuilder(text.Length);
            int index = 0;

            while (index < text.Length)
            {
                var open = text.IndexOf("{{", index, StringComparison.Ordinal);
                if (open < 0)
                {
                    buffer.Append(text, index, text.Length - index);
                    break;
                }

                if (open > index)
                {
                    buffer.Append(text, index, open - index);
                }

                var close = text.IndexOf("}}", open + 2, StringComparison.Ordinal);
                if (close < 0)
                {
                    buffer.Append(text, open, text.Length - open);
                    break;
                }

                FlushBuffer(builder, buffer);

                var tag = text.Substring(open, close - open + 2);
                if (ContainsStatGlyph(tag))
                {
                    builder.Append(tag);
                }
                else
                {
                    buffer.Append(tag);
                }

                index = close + 2;
            }

            FlushBuffer(builder, buffer);
            return builder.ToString();
        }

        private static void FlushBuffer(StringBuilder builder, StringBuilder buffer)
        {
            if (buffer.Length == 0)
            {
                return;
            }

            var chunk = buffer.ToString();
            buffer.Clear();
            var localized = Translator.Instance.Apply(chunk, "Look.DisplayName");
            builder.Append(string.IsNullOrEmpty(localized) ? chunk : localized);
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
