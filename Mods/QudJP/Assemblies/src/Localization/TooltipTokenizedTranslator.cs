using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace QudJP.Localization
{
    internal static class TooltipTokenizedTranslator
    {
        private static readonly Regex SpriteTag = new("<sprite\\b[^>]*?>", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly Regex RichTag = new("</?(?:b|i|u|s|mark|color|size|font|align|alpha|cspace|mspace|indent|line-height|lowercase|uppercase|smallcaps|sub|sup|voffset|link|nobr|br)\\b[^>]*?>", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly Regex QudTag = new("\\{\\{[^}]*\\}\\}", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly Regex PuaChar = new("[\\uE000-\\uF8FF]", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly Regex TokenRegex = new("⟦[SRQP][0-9A-F]+⟧", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public static string Translate(string? value, string contextId)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            var text = value!;
            var tokens = CollectTokens(text);
            if (tokens.Count == 0)
            {
                return SafeStringTranslator.SafeTranslate(text, contextId);
            }

            var tokenized = ReplaceWithTokens(text, tokens);
            var translated = TranslateRuns(tokenized, contextId);
            return RestoreTokens(translated, tokens);
        }

        private static List<TokenInfo> CollectTokens(string value)
        {
            var tokens = new List<TokenInfo>();
            var occupied = value.Length > 0 ? new bool[value.Length] : Array.Empty<bool>();

            Capture(SpriteTag, 'S');
            Capture(RichTag, 'R');
            Capture(QudTag, 'Q');
            Capture(PuaChar, 'P');

            return tokens;

            void Capture(Regex regex, char prefix)
            {
                foreach (Match match in regex.Matches(value))
                {
                    if (!match.Success || match.Length == 0)
                    {
                        continue;
                    }

                    if (HasOverlap(match.Index, match.Length, occupied))
                    {
                        continue;
                    }

                    MarkOccupied(match.Index, match.Length, occupied);
                    var token = $"⟦{prefix}{tokens.Count:X}⟧";
                    tokens.Add(new TokenInfo(match.Index, match.Length, token, match.Value));
                }
            }
        }

        private static bool HasOverlap(int start, int length, bool[] occupied)
        {
            if (occupied.Length == 0)
            {
                return false;
            }

            var end = Math.Min(start + length, occupied.Length);
            for (int i = start; i < end; i++)
            {
                if (occupied[i])
                {
                    return true;
                }
            }

            return false;
        }

        private static void MarkOccupied(int start, int length, bool[] occupied)
        {
            if (occupied.Length == 0)
            {
                return;
            }

            var end = Math.Min(start + length, occupied.Length);
            for (int i = start; i < end; i++)
            {
                occupied[i] = true;
            }
        }

        private static string ReplaceWithTokens(string value, List<TokenInfo> tokens)
        {
            if (tokens.Count == 0)
            {
                return value;
            }

            tokens.Sort((left, right) => left.Start.CompareTo(right.Start));
            var builder = new StringBuilder(value);
            for (int i = tokens.Count - 1; i >= 0; i--)
            {
                var token = tokens[i];
                builder.Remove(token.Start, token.Length);
                builder.Insert(token.Start, token.Token);
            }

            return builder.ToString();
        }

        private static string TranslateRuns(string tokenized, string contextId)
        {
            var builder = new StringBuilder(tokenized.Length + 16);
            int cursor = 0;
            foreach (Match match in TokenRegex.Matches(tokenized))
            {
                if (match.Index > cursor)
                {
                    var length = match.Index - cursor;
                    var segment = tokenized.Substring(cursor, length);
                    AppendTranslated(builder, segment, contextId);
                }

                builder.Append(match.Value);
                cursor = match.Index + match.Length;
            }

            if (cursor < tokenized.Length)
            {
                var tail = tokenized.Substring(cursor);
                AppendTranslated(builder, tail, contextId);
            }

            return builder.ToString();
        }

        private static string RestoreTokens(string value, List<TokenInfo> tokens)
        {
            if (tokens.Count == 0 || string.IsNullOrEmpty(value))
            {
                return value;
            }

            tokens.Sort((left, right) => left.Start.CompareTo(right.Start));
            var builder = new StringBuilder(value);
            foreach (var token in tokens)
            {
                builder.Replace(token.Token, token.Raw);
            }

            return builder.ToString();
        }

        private static void AppendTranslated(StringBuilder builder, string segment, string contextId)
        {
            if (string.IsNullOrEmpty(segment))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(segment))
            {
                builder.Append(segment);
                return;
            }

            var translated = Translator.Instance.Apply(segment, contextId);
            builder.Append(string.IsNullOrEmpty(translated) ? segment : translated);
        }

        private readonly struct TokenInfo
        {
            public TokenInfo(int start, int length, string token, string raw)
            {
                Start = start;
                Length = length;
                Token = token;
                Raw = raw;
            }

            public int Start { get; }
            public int Length { get; }
            public string Token { get; }
            public string Raw { get; }
        }
    }
}
