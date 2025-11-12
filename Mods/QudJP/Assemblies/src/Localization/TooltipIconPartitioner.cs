using System;
using System.Text;

namespace QudJP.Localization
{
    internal static class TooltipIconPartitioner
    {
        public static PartitionResult Partition(string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return PartitionResult.Empty;
            }

            var text = value!;
            if (!ContainsIconCandidate(text))
            {
                return new PartitionResult(text, string.Empty, string.Empty, hasIcons: false);
            }

            var start = 0;
            var prefix = ExtractLeadingIcons(text, ref start);
            var end = text.Length;
            var suffix = ExtractTrailingIcons(text, ref end);

            if (prefix.Length == 0 && suffix.Length == 0)
            {
                return new PartitionResult(text, string.Empty, string.Empty, hasIcons: false);
            }

            var middle = start >= end ? string.Empty : text.Substring(start, end - start);
            return new PartitionResult(middle, prefix.ToString(), suffix.ToString(), hasIcons: true);
        }

        private static bool ContainsIconCandidate(string value)
        {
            if (value.IndexOf("<sprite", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            for (int i = 0; i < value.Length; i++)
            {
                if (IsPrivateUseGlyph(value[i]))
                {
                    return true;
                }
            }

            return false;
        }

        private static StringBuilder ExtractLeadingIcons(string value, ref int index)
        {
            var builder = new StringBuilder();
            while (index < value.Length)
            {
                if (TryConsumeSpriteTag(value, ref index, builder))
                {
                    continue;
                }

                if (IsPrivateUseGlyph(value[index]))
                {
                    builder.Append(value[index]);
                    index++;
                    continue;
                }

                if (builder.Length > 0 && char.IsWhiteSpace(value[index]))
                {
                    builder.Append(value[index]);
                    index++;
                    continue;
                }

                break;
            }

            return builder;
        }

        private static StringBuilder ExtractTrailingIcons(string value, ref int end)
        {
            var builder = new StringBuilder();
            var captured = false;
            while (end > 0)
            {
                if (TryConsumeSpriteTagFromEnd(value, ref end, builder))
                {
                    captured = true;
                    continue;
                }

                var next = value[end - 1];
                if (IsPrivateUseGlyph(next))
                {
                    builder.Insert(0, next);
                    end--;
                    captured = true;
                    continue;
                }

                if (captured && char.IsWhiteSpace(next))
                {
                    builder.Insert(0, next);
                    end--;
                    continue;
                }

                break;
            }

            return builder;
        }

        private static bool TryConsumeSpriteTag(string value, ref int index, StringBuilder buffer)
        {
            if (index >= value.Length || value[index] != '<')
            {
                return false;
            }

            var close = value.IndexOf('>', index);
            if (close < 0)
            {
                return false;
            }

            var length = close - index + 1;
            if (!StartsWith(value, index, "<sprite", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            buffer.Append(value, index, length);
            index += length;
            return true;
        }

        private static bool TryConsumeSpriteTagFromEnd(string value, ref int end, StringBuilder buffer)
        {
            var search = end - 1;
            while (search >= 0 && value[search] != '<')
            {
                search--;
            }

            if (search < 0)
            {
                return false;
            }

            var close = value.IndexOf('>', search);
            if (close < 0 || close >= end)
            {
                return false;
            }

            var length = close - search + 1;
            if (!StartsWith(value, search, "<sprite", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            buffer.Insert(0, value.Substring(search, length));
            end = search;
            return true;
        }

        private static bool StartsWith(string value, int index, string token, StringComparison comparison)
        {
            if (index < 0 || token.Length == 0 || index + token.Length > value.Length)
            {
                return false;
            }

            return string.Compare(value, index, token, 0, token.Length, comparison) == 0;
        }

        private static bool IsPrivateUseGlyph(char c) =>
            c >= '\uE000' && c <= '\uF8FF';

        internal readonly struct PartitionResult
        {
            public static PartitionResult Empty { get; } = new(string.Empty, string.Empty, string.Empty, false);

            public PartitionResult(string label, string prefix, string suffix, bool hasIcons)
            {
                Label = label;
                Prefix = prefix;
                Suffix = suffix;
                HasIcons = hasIcons;
            }

            public string Label { get; }
            public string Prefix { get; }
            public string Suffix { get; }
            public bool HasIcons { get; }
        }
    }
}
