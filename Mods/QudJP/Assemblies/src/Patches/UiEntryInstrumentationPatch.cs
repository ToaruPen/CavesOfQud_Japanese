using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using ModelShark;
using Qud.UI;
using QudJP.Diagnostics;
using TMPro;
using UnityEngine;
using XRL.UI;
using XRL.UI.Framework;

namespace QudJP.Patches
{
    /// <summary>
    /// Adds lightweight instrumentation to UI entry points so we can confirm that Harmony targets
    /// still match the current game build. These prefixes intentionally avoid mutating any strings.
    /// </summary>
    [HarmonyPatch]
    internal static class UiEntryInstrumentationPatch
    {
        [HarmonyPatch(typeof(PopupMessage), nameof(PopupMessage.ShowPopup))]
        private static class PopupMessageInstrumentation
        {
            [HarmonyPrefix]
            [HarmonyPriority(Priority.VeryHigh)]
            private static void Before(
                PopupMessage __instance,
                [HarmonyArgument(0)] string message,
                [HarmonyArgument(5)] string title,
                ref string __state)
            {
                var eid = BeginScope(out var ownsScope);
                __state = ownsScope ? eid : string.Empty;

                BindUITextSkin(__instance?.Message, eid, "TMP.PopupMessage.Body");
                BindUITextSkin(__instance?.Title, eid, "TMP.PopupMessage.Title");
                BindInputField(__instance?.inputBox, eid, "TMP.PopupMessage.Input");
                BindPopupTexts(__instance, eid);

                var popupMap = new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase);
                if (!string.IsNullOrEmpty(message))
                {
                    popupMap["BodyText"] = message;
                }

                if (!string.IsNullOrEmpty(title))
                {
                    popupMap["Title"] = title;
                }

                if (popupMap.Count > 0)
                {
                    TooltipParamMapCache.Remember(eid, popupMap);
                }

                var buttonCount = __instance?.controller?.menuData?.Count ?? 0;
                var itemCount = __instance?.controller?.bottomContextOptions?.Count ?? 0;
                JpLog.Info(
                    eid,
                    "Popup",
                    "HOOK",
                    $"titleLen={title?.Length ?? 0} msgLen={message?.Length ?? 0} buttons={buttonCount} ctxItems={itemCount}");
            }

            [HarmonyPostfix]
            [HarmonyPriority(Priority.VeryLow)]
            private static void After(string __state) => EndScope(__state);
        }

        [HarmonyPatch(typeof(TooltipManager), nameof(TooltipManager.SetTextAndSize))]
        private static class TooltipInstrumentation
        {
            [HarmonyPrefix]
            [HarmonyPriority(Priority.VeryHigh)]
            private static void Before(TooltipTrigger trigger, ref string __state)
            {
                if (trigger == null)
                {
                    __state = string.Empty;
                    return;
                }

                var eid = BeginScope(out var ownsScope);
                __state = ownsScope ? eid : string.Empty;

                var styleName = trigger.tooltipStyle != null ? trigger.tooltipStyle.name : "<null>";
                var paramCount = trigger.parameterizedTextFields?.Count ?? 0;
                var fieldCount = trigger.Tooltip?.TMPFields?.Count ?? 0;

                foreach (var tooltip in TooltipTraversal.EnumerateAll(trigger))
                {
                    var tooltipStyle = TooltipTraversal.ResolveStyleName(tooltip) ?? styleName;
                    BindTooltipCluster(tooltip, eid, tooltipStyle);
                }

                JpLog.Info(
                    eid,
                    "Tooltip",
                    "HOOK",
                    $"style={styleName} trigger={trigger.name ?? "<null>"} params={paramCount} tmpFields={fieldCount}");
            }

            [HarmonyPostfix]
            [HarmonyPriority(Priority.VeryLow)]
            private static void After(string __state) => EndScope(__state);
        }

        [HarmonyPatch(typeof(SelectableTextMenuItem), nameof(SelectableTextMenuItem.SelectChanged))]
        private static class SelectableTextMenuItemInstrumentation
        {
            [HarmonyPrefix]
            [HarmonyPriority(Priority.VeryHigh)]
            private static void Before(SelectableTextMenuItem __instance, [HarmonyArgument(0)] bool newState, ref string __state)
            {
                if (__instance == null)
                {
                    __state = string.Empty;
                    return;
                }

                var eid = BeginScope(out var ownsScope);
                __state = ownsScope ? eid : string.Empty;

                BindUITextSkin(__instance.item, eid, "TMP.SelectableTextMenuItem.Label");

                var menuItem = __instance.data as QudMenuItem?;
                var cmd = menuItem?.command ?? "<null>";
                var len = menuItem?.text?.Length ?? 0;

                JpLog.Info(
                    eid,
                    "Menu",
                    "HOOK",
                    $"state={(newState ? "selected" : "idle")} cmd={cmd} len={len}");
            }

            [HarmonyPostfix]
            [HarmonyPriority(Priority.VeryLow)]
            private static void After(string __state) => EndScope(__state);
        }

        [HarmonyPatch(typeof(InventoryLine), nameof(InventoryLine.setData))]
        private static class InventoryLineInstrumentation
        {
            [HarmonyPrefix]
            [HarmonyPriority(Priority.VeryHigh)]
            private static void Before(InventoryLine __instance, [HarmonyArgument(0)] FrameworkDataElement data, ref string __state)
            {
                if (__instance == null)
                {
                    __state = string.Empty;
                    return;
                }

                var eid = BeginScope(out var ownsScope);
                __state = ownsScope ? eid : string.Empty;

                BindUITextSkin(__instance.categoryLabel, eid, "TMP.InventoryLine.CategoryLabel");
                BindUITextSkin(__instance.categoryExpandLabel, eid, "TMP.InventoryLine.CategoryExpand");
                BindUITextSkin(__instance.categoryWeightText, eid, "TMP.InventoryLine.CategoryWeight");
                BindUITextSkin(__instance.itemWeightText, eid, "TMP.InventoryLine.ItemWeight");
                BindUITextSkin(__instance.text, eid, "TMP.InventoryLine.Description");
                BindUITextSkin(__instance.hotkeyText, eid, "TMP.InventoryLine.Hotkey");

                var payload = data as InventoryLineData;
                var kind = payload?.category == true ? "Category" : "Item";
                var label = payload?.category == true ? (payload.categoryName ?? "<null>") : (payload?.displayName ?? payload?.go?.DisplayName ?? "<null>");
                var weight = payload?.category == true ? payload.categoryWeight : payload?.go?.Weight;

                JpLog.Info(
                    eid,
                    "Inventory",
                    "HOOK",
                    $"kind={kind} label='{label ?? "<null>"}' weight={weight?.ToString() ?? "<null>"}");
            }

            [HarmonyPostfix]
            [HarmonyPriority(Priority.VeryLow)]
            private static void After(string __state) => EndScope(__state);
        }

        private static string BeginScope(out bool ownsScope)
        {
            var current = UIContext.Current;
            if (!string.IsNullOrEmpty(current))
            {
                ownsScope = false;
                return current!;
            }

            ownsScope = true;
            return UIContext.Capture(JpLog.NewEID());
        }

        private static void EndScope(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return;
            }

            UIContext.Release(token);
        }

        private static void BindTooltipCluster(Tooltip tooltip, string eid, string styleName)
        {
            if (tooltip == null)
            {
                return;
            }

            var fields = tooltip.TMPFields;
            if (fields == null)
            {
                goto ScanChildren;
            }

            foreach (var field in fields)
            {
                var text = field?.Text;
                if (text == null)
                {
                    continue;
                }

                var fieldName = text.gameObject?.name ?? "Field";
                var contextId = $"ModelShark.Tooltip.{styleName}.{fieldName}";
                BindTMPText(text, eid, contextId);
            }

        ScanChildren:
            var root = tooltip.GameObject;
            if (root == null)
            {
                return;
            }

            var tmps = root.GetComponentsInChildren<TMP_Text>(includeInactive: true);
            foreach (var text in tmps)
            {
                if (text == null)
                {
                    continue;
                }

                var fieldName = text.gameObject?.name ?? "Field";
                var contextId = $"ModelShark.Tooltip.{styleName}.{fieldName}";
                BindTMPText(text, eid, contextId);
            }
        }


        private static void BindUITextSkin(UITextSkin? skin, string eid, string contextId)
        {
            if (skin == null)
            {
                return;
            }

            var tmp = skin.GetComponent<TMP_Text>();
            BindTMPText(tmp, eid, contextId);
        }

        private static void BindInputField(ControlledTMPInputField? field, string eid, string contextId)
        {
            if (field == null)
            {
                return;
            }

            BindTMPText(field.textComponent, eid, contextId);
        }

        private static void BindPopupTexts(PopupMessage? popup, string eid)
        {
            if (popup == null)
            {
                return;
            }

            var root = popup.gameObject;
            if (root == null)
            {
                return;
            }

            var tmps = root.GetComponentsInChildren<TMP_Text>(includeInactive: true);
            foreach (var text in tmps)
            {
                if (text == null)
                {
                    continue;
                }

                var fieldName = text.gameObject?.name ?? "Field";
                var contextId = $"TMP.PopupMessage.{fieldName}";
                BindTMPText(text, eid, contextId);
            }
        }

        private static void BindTMPText(TMP_Text? text, string eid, string contextId)
        {
            if (text == null)
            {
                return;
            }

            ContextHints.Set(text, contextId);
            UIContext.Bind(text, eid);
        }
    }
}
