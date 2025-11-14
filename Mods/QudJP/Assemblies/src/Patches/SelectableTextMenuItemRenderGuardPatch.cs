using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using HarmonyLib;
using Qud.UI;
using QudJP.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XRL.UI;

namespace QudJP.Patches
{
    /// <summary>
    /// Defensive guard for SelectableTextMenuItem so localized strings don't vanish when TMP truncates or
    /// fails to build geometry (common when the translated text contains multi-byte characters).
    /// </summary>
    [HarmonyPatch(typeof(SelectableTextMenuItem))]
    internal static class SelectableTextMenuItemRenderGuardPatch
    {
        private static int Logged;
        private const int MaxLogs = 24;
        private static int LoggedCharCounts;
        private const int MaxCharCountLogs = 80;
        private static readonly WaitForEndOfFrame WaitFrame = new();
        private static readonly ConditionalWeakTable<SelectableTextMenuItem, PendingMarker> PendingRebuilds = new();
        private static readonly AccessTools.FieldRef<UITextSkin, string?> FormattedTextRef =
            AccessTools.FieldRefAccess<UITextSkin, string>("formattedText");
        private static readonly AccessTools.FieldRef<UITextSkin, string?> LastTextRef =
            AccessTools.FieldRefAccess<UITextSkin, string>("lasttext");

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

            PrepareTextComponent(tmp);

            if (TryBuildMesh(__instance, skin, tmp, phase: "immediate"))
            {
                return;
            }

            if (!ScheduleDeferredRebuild(__instance))
            {
                RecoverWithFallback(__instance, tmp);
            }
        }

        private static bool ScheduleDeferredRebuild(SelectableTextMenuItem menu)
        {
            if (menu == null || !menu.isActiveAndEnabled)
            {
                return false;
            }

            lock (PendingRebuilds)
            {
                if (PendingRebuilds.TryGetValue(menu, out _))
                {
                    return true;
                }

                PendingRebuilds.Add(menu, new PendingMarker());
            }

            menu.StartCoroutine(DeferredRebuild(menu));
            return true;
        }

        private static IEnumerator DeferredRebuild(SelectableTextMenuItem menu)
        {
            yield return null;
            yield return WaitFrame;

            lock (PendingRebuilds)
            {
                PendingRebuilds.Remove(menu);
            }

            var skin = menu?.item;
            var tmp = skin?.GetComponent<TMP_Text>();
            if (tmp == null)
            {
                yield break;
            }

            PrepareTextComponent(tmp);
            ForceSkinRefresh(skin);

            if (TryBuildMesh(menu, skin, tmp, phase: "deferred"))
            {
                yield break;
            }

            RecoverWithFallback(menu, tmp);
        }

        private static void PrepareTextComponent(TMP_Text tmp)
        {
            QudJP.FontManager.Instance.ApplyToText(tmp, forceReplace: true);
            tmp.textWrappingMode = TextWrappingModes.PreserveWhitespace;
            tmp.overflowMode = TextOverflowModes.Overflow;
            var c = tmp.color; c.a = 1f; tmp.color = c;
        }

        private static bool TryBuildMesh(SelectableTextMenuItem menuItem, UITextSkin? skin, TMP_Text tmp, string phase)
        {
            FixCollapsedBounds(tmp);
            Canvas.ForceUpdateCanvases();

            tmp.ForceMeshUpdate(ignoreActiveState: true, forceTextReparsing: true);
            var count = tmp.textInfo?.characterCount ?? 0;
            LogCharCount(tmp, count, phase, tmp.canvasRenderer?.cull ?? false);
            if (count > 0)
            {
                LogSuccess(tmp);
                return true;
            }

            if (ForceSkinRefresh(skin))
            {
                FixCollapsedBounds(tmp);
                Canvas.ForceUpdateCanvases();
                tmp.ForceMeshUpdate(ignoreActiveState: true, forceTextReparsing: true);
                count = tmp.textInfo?.characterCount ?? 0;
                LogCharCount(tmp, count, $"{phase}/skin", tmp.canvasRenderer?.cull ?? false);
                if (count > 0)
                {
                    LogSuccess(tmp);
                    return true;
                }
            }

            return false;
        }

        private static bool ForceSkinRefresh(UITextSkin? skin)
        {
            if (skin == null)
            {
                return false;
            }

            try
            {
                FormattedTextRef(skin) = null;
                LastTextRef(skin) = null;
            }
            catch
            {
                // In case internal layout changed between versions; fall through to Apply.
            }

            skin.Apply();
            return true;
        }

        private static void LogCharCount(TMP_Text tmp, int count, string phase, bool culled)
        {
            if (LoggedCharCounts >= MaxCharCountLogs)
            {
                return;
            }

            LoggedCharCounts++;
            UnityEngine.Debug.Log($"[QudJP][Diag] Menu item mesh ({phase}): obj='{tmp.gameObject.name}', charCount={count}, culled={culled}, text='{Short(tmp.text)}'");
        }

        private static void LogSuccess(TMP_Text tmp)
        {
            if (Logged >= MaxLogs)
            {
                return;
            }

            Logged++;
            UnityEngine.Debug.Log($"[QudJP] Menu item ok: obj='{tmp.gameObject.name}', text='{Short(tmp.text)}'");
        }

        private static void FixCollapsedBounds(TMP_Text tmp)
        {
            var rt = tmp.rectTransform;
            if (rt == null)
            {
                return;
            }

            var rect = rt.rect;
            if (rect.width < 1f || rect.height < 1f)
            {
                var preferred = tmp.GetPreferredValues(Mathf.Max(8f, rect.width), Mathf.Max(8f, rect.height));
                rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Max(preferred.x, 48f));
                rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Max(preferred.y, 16f));
            }

            LayoutRebuilder.MarkLayoutForRebuild(rt);
            LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
        }

        private static void RecoverWithFallback(SelectableTextMenuItem? menuItem, TMP_Text tmp)
        {
            PrepareTextComponent(tmp);
            tmp.textWrappingMode = TextWrappingModes.PreserveWhitespace;
            tmp.overflowMode = TextOverflowModes.Overflow;
            tmp.text = BuildFallbackText(menuItem);
            tmp.ForceMeshUpdate(ignoreActiveState: true, forceTextReparsing: true);

            if (Logged < MaxLogs)
            {
                Logged++;
                UnityEngine.Debug.Log($"[QudJP] Menu item recovered with fallback: obj='{tmp.gameObject.name}', after='{Short(tmp.text)}'");
            }
        }

        private static readonly Regex HotkeyRegex = new(@"^\s*(\[[^\]]+\])(\s*)(.*)$", RegexOptions.Compiled);

        private static string BuildFallbackText(SelectableTextMenuItem? menuItem)
        {
            if (menuItem?.data is QudMenuItem item && !string.IsNullOrWhiteSpace(item.text))
            {
                return HighlightHotkey(StripMarkup(item.text), item);
            }

            return "<Missing>";
        }

        private static string StripMarkup(string value)
        {
            var stripped = Regex.Replace(value, "\\{\\{[^|{}]+\\|", string.Empty);
            stripped = stripped.Replace("{{", string.Empty).Replace("}}", string.Empty);
            return stripped.Trim();
        }

        private static string HighlightHotkey(string value, QudMenuItem? source)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            var match = HotkeyRegex.Match(value);
            if (!match.Success)
            {
                var trimmed = value.TrimStart();
                if (string.IsNullOrEmpty(trimmed))
                {
                    return value;
                }

                var firstTokenEnd = trimmed.IndexOf(' ');
                var firstToken = firstTokenEnd > 0 ? trimmed.Substring(0, firstTokenEnd) : trimmed;
                if (firstToken.Length > 16)
                {
                    return value;
                }

                var idx = value.IndexOf(firstToken, StringComparison.Ordinal);
                if (idx < 0)
                {
                    return value;
                }

                var resolvedLabel = MenuHotkeyHelper.GetPrimaryLabel(source?.command, source?.hotkey);
                var replacement = string.IsNullOrWhiteSpace(resolvedLabel) ? firstToken : resolvedLabel!;

                return value.Substring(0, idx) +
                    $"<color=#CFC041FF>[{replacement}]</color>" +
                    value.Substring(idx + firstToken.Length);
            }

            var hotkey = match.Groups[1].Value;
            var spacing = match.Groups[2].Value;
            var rest = match.Groups[3].Value;
            var finalLabel = MenuHotkeyHelper.GetPrimaryLabel(source?.command, source?.hotkey);
            var emphasized = string.IsNullOrWhiteSpace(finalLabel) ? hotkey : $"[{finalLabel!.Trim()}]";
            return $"<color=#CFC041FF>{emphasized}</color>{spacing}{rest}";
        }

        private static string Short(string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return "<empty>";
            }

            return value.Length > 120 ? value.Substring(0, 120) + "..." : value;
        }

        private sealed class PendingMarker
        {
        }
    }
}
