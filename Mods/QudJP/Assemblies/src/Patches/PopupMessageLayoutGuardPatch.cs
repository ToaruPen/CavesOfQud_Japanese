using System.Collections;
using System.Collections.Generic;
using HarmonyLib;
using Qud.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace QudJP.Patches
{
    /// <summary>
    /// PopupMessage.ShowPopup の直後に menuData / bottom-context 両方の TMP_Text を再レイアウトし、
    /// RectMask2D のカリング状態をリセットして翻訳済みテキストが描画されるようにする。
    /// </summary>
    [HarmonyPatch(typeof(PopupMessage))]
    internal static class PopupMessageLayoutGuardPatch
    {
        private static readonly WaitForEndOfFrame WaitFrame = new();

        [HarmonyPostfix]
        [HarmonyPatch(nameof(PopupMessage.ShowPopup))]
        private static void AfterShow(PopupMessage __instance)
        {
            if (__instance == null || !__instance.gameObject.activeInHierarchy)
            {
                return;
            }

            __instance.StartCoroutine(ReflowPopup(__instance));
        }

        private static IEnumerator ReflowPopup(PopupMessage popup)
        {
            // 1 フレーム分だけ待ち、Unity 側の初期レイアウトを完了させる。
            yield return null;
            yield return WaitFrame;

            if (popup == null || !popup.gameObject.activeInHierarchy)
            {
                yield break;
            }

            RefreshMenuArea(popup);
            RefreshBottomContext(popup);

            popup.controller?.UpdateButtonLayout();
            RebuildRect(popup.transform as RectTransform);
            Canvas.ForceUpdateCanvases();
        }

        private static void RefreshMenuArea(PopupMessage popup)
        {
            var controller = popup.controller;
            if (controller == null)
            {
                return;
            }

            controller.UpdateElements(evenIfNotCurrent: true);

            var menuRect = controller.menuItemContainer != null
                ? controller.menuItemContainer.GetComponent<RectTransform>()
                : controller.transform as RectTransform;

            RebuildRect(menuRect);
            NotifyMaskState(menuRect);

            var menuItems = controller.menuItemContainer != null
                ? controller.menuItemContainer.GetComponentsInChildren<SelectableTextMenuItem>(includeInactive: true)
                : controller.GetComponentsInChildren<SelectableTextMenuItem>(includeInactive: true);

            RefreshSelectableTexts(menuItems);
        }

        private static void RefreshBottomContext(PopupMessage popup)
        {
            var bottom = popup.bottomContextController;
            if (bottom == null)
            {
                return;
            }

            bottom.RefreshButtons();

            var rect = bottom.rightBar != null
                ? bottom.rightBar.transform?.parent as RectTransform
                : bottom.GetComponent<RectTransform>();

            RebuildRect(rect);
            NotifyMaskState(rect);

            RefreshSelectableTexts(bottom.buttons);
        }

        private static void RefreshSelectableTexts(IEnumerable<SelectableTextMenuItem>? items)
        {
            if (items == null)
            {
                return;
            }

            foreach (var selectable in items)
            {
                var tmp = selectable?.item != null
                    ? selectable.item.GetComponent<TMP_Text>()
                    : null;
                if (tmp == null)
                {
                    continue;
                }

                tmp.ForceMeshUpdate(ignoreActiveState: true, forceTextReparsing: true);
                if (tmp is MaskableGraphic maskable)
                {
                    maskable.RecalculateClipping();
                    maskable.canvasRenderer.cull = false;
                }
            }
        }

        private static void RebuildRect(RectTransform? rect)
        {
            if (rect == null)
            {
                return;
            }

            LayoutRebuilder.MarkLayoutForRebuild(rect);
            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
        }

        private static void NotifyMaskState(Component? anchor)
        {
            if (anchor == null)
            {
                return;
            }

            var mask = anchor.GetComponent<RectMask2D>() ?? anchor.GetComponentInParent<RectMask2D>();
            if (mask == null)
            {
                return;
            }

            mask.enabled = false;
            mask.enabled = true;
            MaskUtilities.Notify2DMaskStateChanged(mask);
        }
    }
}
