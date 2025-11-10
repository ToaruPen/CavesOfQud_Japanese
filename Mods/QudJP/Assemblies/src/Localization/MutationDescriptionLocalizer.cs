using System;

namespace QudJP.Localization
{
    /// <summary>
    /// Provides mutation-specific long description translations for the character builder.
    /// </summary>
    internal static class MutationDescriptionLocalizer
    {
        private const string ContextId = "Chargen.Mutation.LongDescription";
        private const string KeyPrefix = "mutation:";

        public static string Localize(string? mutationName, string? originalLongDescription)
        {
            if (!string.IsNullOrEmpty(mutationName))
            {
                var token = KeyPrefix + mutationName;
                if (TryTranslate(token, out var localizedByKey))
                {
                    return localizedByKey;
                }
            }

            if (TryTranslate(originalLongDescription, out var localizedByValue))
            {
                return localizedByValue;
            }

            return originalLongDescription?.Replace("\r\n", "\n").Replace("\r", "\n") ?? string.Empty;
        }

        private static bool TryTranslate(string? token, out string localized)
        {
            localized = string.Empty;
            if (string.IsNullOrEmpty(token))
            {
                return false;
            }

            var translated = SafeStringTranslator.SafeTranslate(token, ContextId);
            if (string.IsNullOrEmpty(translated) || string.Equals(translated, token, StringComparison.Ordinal))
            {
                return false;
            }

            localized = translated.Replace("\r\n", "\n").Replace("\r", "\n");
            return true;
        }
    }
}
