using HarmonyLib;
using Qud.UI;
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
                [HarmonyArgument(5)] ref string title)
            {
                message = LocalizeUIPopupMessage(message);
                title = LocalizeUIPopupTitle(title);
            }
        }

        private static string LocalizeUIPopupMessage(string message)
        {
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
    }
}
