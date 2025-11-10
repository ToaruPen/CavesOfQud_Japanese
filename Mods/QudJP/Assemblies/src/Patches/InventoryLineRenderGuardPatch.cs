using HarmonyLib;
using Qud.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XRL.UI.Framework;

namespace QudJP.Patches
{
    /// <summary>
    /// Defensive tweaks after InventoryLine.setData to avoid invisible item labels
    /// due to wrapping/overflow/layout quirks with TMP + UITextSkin.
    /// Category rows are left untouched.
    /// </summary>
    [HarmonyPatch(typeof(InventoryLine))]
    internal static class InventoryLineRenderGuardPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(InventoryLine.setData))]
        private static void AfterSetData(InventoryLine __instance, FrameworkDataElement data)
        {
            if (__instance == null || data is not InventoryLineData line || line.category)
            {
                return;
            }

            var skin = __instance.text;
            if (skin == null)
            {
                return;
            }

            // Disable block wrapping for item rows; rely on TMP for layout.
            skin.useBlockWrap = false;

            var tmp = skin.GetComponent<TMP_Text>();
            if (tmp == null)
            {
                return;
            }

            // Ensure text is visible and not clipped aggressively.
            tmp.overflowMode = TextOverflowModes.Overflow;
            tmp.enableWordWrapping = false;
            var c = tmp.color; c.a = 1f; tmp.color = c;

            // Nudge layout to recompute sizes in case rect was 0-width.
            var rt = tmp.rectTransform;
            if (rt != null)
            {
                LayoutRebuilder.MarkLayoutForRebuild(rt);
            }
        }
    }
}

