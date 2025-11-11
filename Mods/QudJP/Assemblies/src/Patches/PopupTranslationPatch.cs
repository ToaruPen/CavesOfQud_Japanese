using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using HarmonyLib;
using Qud.UI;
using QudJP.Diagnostics;
using QudJP.Localization;
using XRL.UI;

namespace QudJP.Patches
{
    /// <summary>
    /// Translate console Popup and modern UI PopupMessage with dictionary + simple rules.
    /// </summary>
    internal static class PopupTranslationPatch
    {
        [HarmonyPatch(typeof(Popup), nameof(Popup.Show))]
        private static class PopupShowPatch
        {
            [HarmonyPrefix]
            private static void Prefix(
                [HarmonyArgument(0)] ref string Text,
                [HarmonyArgument(2)] ref string Caption)
            {
                Text = SafeStringTranslator.SafeTranslate(Text, "Popup.Show.Text");
                Caption = SafeStringTranslator.SafeTranslate(Caption, "Popup.Show.Caption");
            }
        }

        [HarmonyPatch(typeof(Popup), nameof(Popup.AskString))]
        private static class PopupAskStringPatch
        {
            [HarmonyPrefix]
            private static void Prefix(
                [HarmonyArgument(0)] ref string Prompt,
                [HarmonyArgument(4)] ref string Title)
            {
                Prompt = SafeStringTranslator.SafeTranslate(Prompt, "Popup.AskString.Prompt");
                Title = SafeStringTranslator.SafeTranslate(Title, "Popup.AskString.Title");
            }
        }

        // Modern UI popup (Qud.UI.PopupMessage)
        [HarmonyPatch(typeof(PopupMessage), nameof(PopupMessage.ShowPopup))]
        private static class PopupMessageShowPatch
        {
            [HarmonyPrefix]
            private static void Prefix(
                [HarmonyArgument(0)] ref string message,
                [HarmonyArgument(1)] List<QudMenuItem>? buttons,
                [HarmonyArgument(3)] List<QudMenuItem>? items,
                [HarmonyArgument(5)] ref string title,
                ref string __state)
            {
                var eid = UIContext.Capture(JpLog.NewEID());
                __state = eid;

                JpLog.Info(eid, "Popup", "START", $"title='{title ?? "<null>"}' msgLen={message?.Length ?? 0}");
                message = LocalizeUIPopupMessage(message ?? string.Empty);
                title = LocalizeUIPopupTitle(title ?? string.Empty);

                NormalizeHotkeys(buttons);
                NormalizeHotkeys(items);
            }

            [HarmonyPostfix]
            private static void Postfix(
                [HarmonyArgument(0)] string message,
                [HarmonyArgument(5)] string title,
                string __state)
            {
                if (string.IsNullOrEmpty(__state))
                {
                    return;
                }

                JpLog.Info(__state, "Popup", "END", $"title='{title ?? "<null>"}' msgLen={message?.Length ?? 0}");
                UIContext.Release(__state);
            }
        }

        private static void NormalizeHotkeys(List<QudMenuItem>? entries)
        {
            if (entries == null || entries.Count == 0)
            {
                return;
            }

            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                var normalized = MenuItemTextLocalizer.Apply(entry.text, entry.command, entry.hotkey);
                if (string.Equals(normalized, entry.text, StringComparison.Ordinal))
                {
                    continue;
                }

                entry.text = normalized;
                entries[i] = entry;
            }
        }

        private static string LocalizeUIPopupMessage(string message)
        {
            if (TryLocalizeQuitWarning(message, out var manual))
            {
                return manual;
            }

            var translated = SafeStringTranslator.SafeTranslate(message, "PopupMessage.ShowPopup.Message");
            if (!string.Equals(translated, message, System.StringComparison.Ordinal))
            {
                return translated;
            }

            const string DeleteConfirmPrefix = "Are you sure you want to delete the save game for ";
            if (message.StartsWith(DeleteConfirmPrefix, System.StringComparison.Ordinal) && message.EndsWith("?", System.StringComparison.Ordinal))
            {
                var name = message.Substring(DeleteConfirmPrefix.Length, message.Length - DeleteConfirmPrefix.Length - 1);
                return $"セーブデータ『{name}』を本当に削除しますか？";
            }

            if (string.Equals(message, "Game Deleted!", System.StringComparison.Ordinal))
            {
                return "セーブを削除しました！";
            }

            return message;
        }

        private static string LocalizeUIPopupTitle(string title)
        {
            var translated = SafeStringTranslator.SafeTranslate(title, "PopupMessage.ShowPopup.Title");
            if (!string.Equals(translated, title, System.StringComparison.Ordinal))
            {
                return translated;
            }

            var idx = title.IndexOf("Delete ", System.StringComparison.Ordinal);
            if (idx >= 0)
            {
                return title.Substring(0, idx) + "削除 " + title.Substring(idx + "Delete ".Length);
            }

            return title;
        }

        private static bool TryLocalizeQuitWarning(string message, out string translated)
        {
            translated = string.Empty;
            if (string.IsNullOrEmpty(message))
            {
                return false;
            }

            var normalized = NormalizeMultiline(message);
            if (normalized.IndexOf("if you quit without saving, you will lose all your unsaved progress", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                translated = "セーブせずに終了すると保存されていない進行状況がすべて失われます。本当に終了してよろしいですか？\n\n「QUIT」と入力すると確定します。";
                return true;
            }

            if (normalized.IndexOf("if you quit without saving, you will lose all your progress and your character will be lost", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                translated = "セーブせずに終了すると進行状況とキャラクターが完全に失われます。本当に終了してよろしいですか？\n\n「QUIT」と入力すると確定します。";
                return true;
            }

            return false;
        }

        private static string NormalizeMultiline(string value)
        {
            var collapsed = value.Replace("\r\n", "\n").Replace("\r", "\n");
            collapsed = Regex.Replace(collapsed, "[ \t]+", " ");
            collapsed = Regex.Replace(collapsed, "\n+", "\n");
            return collapsed.Trim();
        }
    }
}
