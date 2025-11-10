using HarmonyLib;
using Qud.UI;
using UnityEngine;
using XRL.UI.Framework;

namespace QudJP.Patches
{
    /// <summary>
    /// Replaces empty message log entries with a visible placeholder so UITextSkin never receives blank text.
    /// </summary>
    [HarmonyPatch(typeof(MessageLogLine))]
    internal static class MessageLogLineLocalizationPatch
    {
        private const string Placeholder = "[missing log entry]";
        private const int MaxLoggedWarnings = 10;
        private static int _loggedWarnings;

        [HarmonyPrefix]
        [HarmonyPatch(nameof(MessageLogLine.setData))]
        private static void EnsureMessageText(FrameworkDataElement data)
        {
            if (data is not MessageLogLineData entry)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(entry.text))
            {
                return;
            }

            entry.text = Placeholder;
            entry.sortText = Placeholder.ToLowerInvariant();

            if (_loggedWarnings >= MaxLoggedWarnings)
            {
                return;
            }

            _loggedWarnings++;
            Debug.LogWarning("[QudJP] MessageLogLine received empty text; substituted placeholder.");
        }
    }
}
