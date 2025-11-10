using System;
using System.Collections.Generic;
using HarmonyLib;
using Qud.UI;
using TMPro;

namespace QudJP.Patches
{
    /// <summary>
    /// High-fidelity diagnostics for popup buttons and bottom context to pinpoint where
    /// menu item text becomes empty. Purely observational; makes no functional changes.
    /// </summary>
    [HarmonyPatch]
    internal static class PopupDiagnosticsPatch
    {
        private const int MaxLogs = 120;
        private static int _logged;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PopupMessage), nameof(PopupMessage.ShowPopup))]
        private static void BeforePopupShow(
            [HarmonyArgument(0)] string message,
            [HarmonyArgument(1)] List<QudMenuItem> buttons,
            [HarmonyArgument(3)] List<QudMenuItem> items,
            [HarmonyArgument(5)] string title)
        {
            if (_logged >= MaxLogs)
            {
                return;
            }

            try
            {
                _logged++;
                UnityEngine.Debug.Log($"[QudJP][Diag] PopupMessage.ShowPopup: title='{title ?? "<null>"}' message='{Trim(message)}'");
                if (buttons != null)
                {
                    int i = 0;
                    foreach (var b in buttons)
                    {
                        UnityEngine.Debug.Log($"[QudJP][Diag]  buttons[{i}]: text='{Trim(b.text)}' cmd='{b.command}' hotkey='{b.hotkey}'");
                        i++;
                    }
                }

                if (items != null)
                {
                    int i = 0;
                    foreach (var it in items)
                    {
                        UnityEngine.Debug.Log($"[QudJP][Diag]  items[{i}]: text='{Trim(it.text)}' cmd='{it.command}' hotkey='{it.hotkey}'");
                        i++;
                    }
                }
            }
            catch { }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(QudMenuBottomContext), nameof(QudMenuBottomContext.RefreshButtons))]
        private static void AfterBottomContextRefresh(QudMenuBottomContext __instance)
        {
            if (_logged >= MaxLogs)
            {
                return;
            }

            try
            {
                var items = __instance?.items;
                var buttons = __instance?.buttons;
                if (items == null || buttons == null)
                {
                    return;
                }

                int count = Math.Min(items.Count, buttons.Count);
                for (int i = 0; i < count; i++)
                {
                    var src = items[i];
                    var btn = buttons[i];
                    var skin = btn != null ? btn.item : null;
                    var tmp = skin != null ? skin.GetComponent<TMP_Text>() : null;
                    string rendered = tmp != null ? tmp.text : "<no-tmp>";
                    UnityEngine.Debug.Log($"[QudJP][Diag] BottomContext map[{i}]: src='{Trim(src.text)}' -> tmp='{Trim(rendered)}' cmd='{src.command}' hotkey='{src.hotkey}'");
                }
            }
            catch { }
        }

        private static string Trim(string s)
        {
            if (string.IsNullOrEmpty(s)) return "<empty>";
            var t = s;
            if (t.Length > 180) t = t.Substring(0, 180) + "...";
            return t;
        }
    }
}
