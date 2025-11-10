using HarmonyLib;
using Qud.UI;
using TMPro;
using XRL.UI.Framework;

namespace QudJP.Patches
{
    /// <summary>
    /// Temporary diagnostics to understand why inventory item names
    /// are not appearing. Logs only a small number of lines per run.
    /// </summary>
    [HarmonyPatch(typeof(InventoryLine))]
    internal static class InventoryLineDebugPatch
    {
        private static int Logged;
        private const int MaxLogs = 24;

        [HarmonyPostfix]
        [HarmonyPatch("setData")]
        private static void AfterSetData(InventoryLine __instance, FrameworkDataElement data)
        {
            if (Logged >= MaxLogs)
            {
                return;
            }

            if (data is not InventoryLineData line || line.category || line.go == null)
            {
                return;
            }

            Logged++;
            var name = line.displayName ?? "<null>";
            var raw = line.go.DisplayName ?? "<null>";
            var id = line.go.IDIfAssigned ?? line.go.DebugName ?? line.go.Blueprint;

            var tmp = __instance?.text?.GetComponent<TMP_Text>();
            var rendered = tmp?.text ?? "<null>";
            if (rendered.Length > 256)
            {
                rendered = rendered.Substring(0, 256) + "...";
            }

            float a = -1f;
            string color = "<null>";
            string size = "<null>";
            string modes = "<null>";
            if (tmp != null)
            {
                var c = tmp.color;
                a = c.a;
                color = $"rgba({c.r:F2},{c.g:F2},{c.b:F2},{c.a:F2})";
                var rt = tmp.rectTransform;
                size = rt != null ? $"{rt.rect.width:F1}x{rt.rect.height:F1}" : "<none>";
                modes = $"wrap={tmp.textWrappingMode}, overflow={tmp.overflowMode}";
            }

            var cg = __instance?.GetComponentInParent<UnityEngine.CanvasGroup>();
            var cgAlpha = cg != null ? cg.alpha.ToString("F2") : "<none>";

            UnityEngine.Debug.Log($"[QudJP] InventoryLine setData: id={id}, displayName='{name}', go.DisplayName='{raw}', tmp.Text='{rendered}', color={color}, rect={size}, modes={modes}, canvasAlpha={cgAlpha}");
        }
    }
}
