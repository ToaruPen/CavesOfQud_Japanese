using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XRL.UI;

namespace QudJP
{
    /// <summary>
    /// Ensures legacy UITextSkin instances also receive the JP font even if they were enabled before our OnEnable patches ran.
    /// </summary>
    [HarmonyPatch(typeof(UITextSkin))]
    internal static class UITextSkinFontPatch
    {
        private static readonly AccessTools.FieldRef<UITextSkin, TextMeshProUGUI?> TmpField =
            AccessTools.FieldRefAccess<UITextSkin, TextMeshProUGUI?>("_tmp");

        [HarmonyPostfix]
        [HarmonyPatch("Apply")]
        private static void ApplyFont(UITextSkin __instance)
        {
            var tmp = TmpField(__instance) ?? __instance.GetComponent<TextMeshProUGUI>();
            if (tmp == null)
            {
                return;
            }

            FontManager.Instance.ApplyToText(tmp, forceReplace: false);
            ConfigureWrapping(tmp);
            EnsureRectSize(tmp);
        }

        private static void ConfigureWrapping(TMP_Text tmp)
        {
            tmp.textWrappingMode = TextWrappingModes.PreserveWhitespace;
            tmp.overflowMode = TextOverflowModes.Overflow;
            tmp.wordWrappingRatios = Mathf.Clamp(tmp.wordWrappingRatios, 0.25f, 1f);
            tmp.ForceMeshUpdate();
        }

        private static void EnsureRectSize(TMP_Text tmp)
        {
            var rt = tmp.rectTransform;
            if (rt == null)
            {
                return;
            }

            var rect = rt.rect;
            if (rect.width > 1f && rect.height > 1f)
            {
                return;
            }

            // Recalculate preferred size based on current content to avoid zero-area rects that clip text.
            var preferred = tmp.GetPreferredValues(Mathf.Max(8f, rect.width), Mathf.Max(8f, rect.height));
            var minWidth = Mathf.Max(preferred.x, 48f);
            var minHeight = Mathf.Max(preferred.y, 16f);

            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, minWidth);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, minHeight);
            LayoutRebuilder.MarkLayoutForRebuild(rt);
            LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
            tmp.ForceMeshUpdate();
            var canvas = rt.GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                Canvas.ForceUpdateCanvases();
            }
        }
    }
}
