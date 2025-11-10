using System;

namespace QudJP.Localization
{
    internal static class SafeStringTranslator
    {
        public static string SafeTranslate(string? value, string contextId)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value ?? string.Empty;
            }

            var translated = Translator.Instance.Apply(value, contextId);
            if (string.IsNullOrEmpty(translated))
            {
                return value ?? string.Empty;
            }

            if (!MarkupLooksBalanced(translated))
            {
                return value ?? string.Empty;
            }

            return translated ?? (value ?? string.Empty);
        }

        private static bool MarkupLooksBalanced(string value)
        {
            int balance = 0;
            for (int i = 0; i < value.Length - 1; i++)
            {
                if (value[i] == '{' && value[i + 1] == '{')
                {
                    balance++;
                }
                else if (value[i] == '}' && value[i + 1] == '}')
                {
                    balance = Math.Max(0, balance - 1);
                }
            }

            return balance == 0;
        }
    }
}
