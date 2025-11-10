using System;

namespace QudJP.Localization
{
    internal static class TokenNormalizer
    {
        public static bool MightContainQudMarkupOrTokens(string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            var text = value!;
            return text.IndexOf("{{", StringComparison.Ordinal) >= 0 ||
                   text.IndexOf("}}", StringComparison.Ordinal) >= 0 ||
                   text.IndexOf("&", StringComparison.Ordinal) >= 0;
        }

        public static string TryNormalize(string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value ?? string.Empty;
            }

            var text = value!;

            if (MightContainQudMarkupOrTokens(text))
            {
                return text;
            }

            return text.Replace("\r\n", "\n").Replace("\r", "\n");
        }
    }
}
