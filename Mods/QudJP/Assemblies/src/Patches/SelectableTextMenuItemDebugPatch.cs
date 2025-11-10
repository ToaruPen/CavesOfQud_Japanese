using HarmonyLib;
using Qud.UI;

namespace QudJP.Patches
{
    /// <summary>
    /// Pinpoint SelectableTextMenuItem.SelectChanged inputs to see when/why data.text is empty.
    /// </summary>
    [HarmonyPatch(typeof(SelectableTextMenuItem))]
    internal static class SelectableTextMenuItemDebugPatch
    {
        private static int Logged;
        private const int MaxLogs = 80;

        [HarmonyPrefix]
        [HarmonyPatch(nameof(SelectableTextMenuItem.SelectChanged))]
        private static void BeforeSelectChanged(SelectableTextMenuItem __instance)
        {
            if (Logged >= MaxLogs)
            {
                return;
            }

            try
            {
                var hasData = __instance?.data is QudMenuItem;
                var item = hasData ? (QudMenuItem)__instance.data : default;
                var text = hasData ? item.text : "<no-data>";
                var cmd = hasData ? item.command : "<no-data>";
                var hk  = hasData ? item.hotkey : "<no-data>";
                UnityEngine.Debug.Log($"[QudJP][Diag] SelectChanged(before): hasData={hasData} text='{Short(text)}' cmd='{cmd}' hotkey='{hk}'");
                Logged++;
            }
            catch { }
        }

        private static string Short(string s)
        {
            if (string.IsNullOrEmpty(s)) return "<empty>";
            var t = s;
            if (t.Length > 140) t = t.Substring(0, 140) + "...";
            return t;
        }
    }
}

