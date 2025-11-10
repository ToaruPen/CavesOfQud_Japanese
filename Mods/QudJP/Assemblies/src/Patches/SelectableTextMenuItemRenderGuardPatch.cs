using HarmonyLib;
using Qud.UI;
using TMPro;

namespace QudJP.Patches
{
    /// <summary>
    /// Guard against empty-render menu items caused by block wrapping + TMP clipping.
    /// If the composed source text is非空なのに TMP が空の場合にのみ緩やかに介入します。
    /// </summary>
    [HarmonyPatch(typeof(SelectableTextMenuItem))]
    internal static class SelectableTextMenuItemRenderGuardPatch
    {
        private static int Logged;
        private const int MaxLogs = 24;
        [HarmonyPostfix]
        [HarmonyPatch(nameof(SelectableTextMenuItem.SelectChanged))]
        private static void AfterSelectChanged(SelectableTextMenuItem __instance)
        {
            var skin = __instance?.item;
            if (skin == null)
            {
                return;
            }

            var tmp = skin.GetComponent<TMP_Text>();
            if (tmp == null)
            {
                return;
            }

            // Ensure Japanese glyphs are available even if this TMP missed OnEnable.
            QudJP.FontManager.Instance.ApplyToText(tmp);

            // 文字列が空のときだけガードを適用する（通常の折返しを尊重）
            if (!string.IsNullOrEmpty(tmp.text))
            {
                if (Logged < MaxLogs)
                {
                    Logged++;
                    UnityEngine.Debug.Log($"[QudJP] Menu item ok: obj='{tmp.gameObject.name}', text='{(tmp.text.Length>120?tmp.text.Substring(0,120)+"...":tmp.text)}'");
                }
                return;
            }

            // 再適用: 折返しを切り、オーバーフローにして描画を優先。
            skin.useBlockWrap = false;
            tmp.textWrappingMode = TextWrappingModes.PreserveWhitespace;
            tmp.overflowMode = TextOverflowModes.Overflow;
            var c = tmp.color; c.a = 1f; tmp.color = c;
            skin.Apply();

            if (Logged < MaxLogs)
            {
                Logged++;
                UnityEngine.Debug.Log($"[QudJP] Menu item guarded: obj='{tmp.gameObject.name}', after='{(tmp.text?.Length>120?tmp.text.Substring(0,120)+"...":tmp.text)}'");
            }
        }
    }
}
