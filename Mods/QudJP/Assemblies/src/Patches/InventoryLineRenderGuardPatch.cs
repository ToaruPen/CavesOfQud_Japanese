using HarmonyLib;
using Qud.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XRL.UI.Framework;

namespace QudJP.Patches
{
    /// <summary>
    /// Defensive tweaks after InventoryLine.setData to avoid invisible labels
    /// due to wrapping/overflow/layout quirks with TMP + UITextSkin.
    /// Applies to both category headers and regular item rows.
    /// </summary>
    [HarmonyPatch(typeof(InventoryLine))]
    internal static class InventoryLineRenderGuardPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(InventoryLine.setData))]
        private static void AfterSetData(InventoryLine __instance, FrameworkDataElement data)
        {
            if (__instance == null || data is not InventoryLineData line)
            {
                return;
            }

            var skin = __instance.text;
            if (skin == null)
            {
                return;
            }

            // Disable block wrapping for all rows; rely on TMP for layout and guards below.
            skin.useBlockWrap = false;

            var tmp = skin.GetComponent<TMP_Text>();
            if (tmp == null)
            {
                return;
            }

            GuardTmpText(tmp);

            GuardTmpText(__instance.itemWeightText?.GetComponent<TMP_Text>());
            GuardTmpText(__instance.categoryWeightText?.GetComponent<TMP_Text>());
        }

        private static void GuardTmpText(TMP_Text? tmp)
        {
            if (tmp == null)
            {
                return;
            }

            QudJP.FontManager.Instance.ApplyToText(tmp);
            tmp.textWrappingMode = TextWrappingModes.PreserveWhitespace;
            tmp.overflowMode = TextOverflowModes.Overflow;
            var c = tmp.color; c.a = 1f; tmp.color = c;
            EnsureRectSize(tmp);

            var rt = tmp.rectTransform;
            if (rt != null)
            {
                LayoutRebuilder.MarkLayoutForRebuild(rt);
                LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
            }
        }

        private static void EnsureRectSize(TMP_Text tmp)
        {
            var rt = tmp.rectTransform;
            if (rt == null)
            {
                return;
            }

            var rect = rt.rect;
            if (rect.width >= 1f && rect.height >= 1f)
            {
                return;
            }

            var preferred = tmp.GetPreferredValues(Mathf.Max(8f, rect.width), Mathf.Max(8f, rect.height));
            var width = Mathf.Max(preferred.x, 64f);
            var height = Mathf.Max(preferred.y, 16f);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }
    }
}
