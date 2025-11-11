using System.Text.RegularExpressions;
using HarmonyLib;
using Qud.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

            // Ensure JP glyphs even when the component missed OnEnable.
            QudJP.FontManager.Instance.ApplyToText(tmp);

            if (!string.IsNullOrEmpty(tmp.text))
            {
                FixCollapsedBounds(tmp);

                tmp.ForceMeshUpdate(ignoreActiveState: true, forceTextReparsing: true);
                var charCount = tmp.textInfo != null ? tmp.textInfo.characterCount : 0;
                if (charCount == 0)
                {
                    RecoverWithFallback(__instance, tmp);
                    return;
                }

                if (Logged < MaxLogs)
                {
                    Logged++;
                    UnityEngine.Debug.Log($"[QudJP] Menu item ok: obj='{tmp.gameObject.name}', text='{Short(tmp.text)}'");
                }
                return;
            }

            // If text actually disappeared, disable block wrap and rebuild with a stripped fallback.
            skin.useBlockWrap = false;
            tmp.textWrappingMode = TextWrappingModes.PreserveWhitespace;
            tmp.overflowMode = TextOverflowModes.Overflow;
            var c = tmp.color; c.a = 1f; tmp.color = c;
            skin.Apply();

            tmp.text = BuildFallbackText(__instance);
            tmp.ForceMeshUpdate(ignoreActiveState: true, forceTextReparsing: true);

            if (Logged < MaxLogs)
            {
                Logged++;
                UnityEngine.Debug.Log($"[QudJP] Menu item guarded: obj='{tmp.gameObject.name}', after='{Short(tmp.text)}'");
            }
        }

        private static void FixCollapsedBounds(TMP_Text tmp)
        {
            var rt = tmp.rectTransform;
            if (rt == null)
            {
                return;
            }

            if (rt.rect.width >= 1f && rt.rect.height >= 1f)
            {
                return;
            }

            var preferred = tmp.GetPreferredValues(Mathf.Max(8f, rt.rect.width), Mathf.Max(8f, rt.rect.height));
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Max(preferred.x, 48f));
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Max(preferred.y, 16f));
            LayoutRebuilder.MarkLayoutForRebuild(rt);
            LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
        }

        private static void RecoverWithFallback(SelectableTextMenuItem? menuItem, TMP_Text tmp)
        {
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
                return HighlightHotkey(StripMarkup(item.text));
            }

            return "<Missing>";
        }

        private static string StripMarkup(string value)
        {
            var stripped = Regex.Replace(value, "\\{\\{[^|{}]+\\|", string.Empty);
            stripped = stripped.Replace("{{", string.Empty).Replace("}}", string.Empty);
            return stripped.Trim();
        }

        private static string HighlightHotkey(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            var match = HotkeyRegex.Match(value);
            if (!match.Success)
            {
                return value;
            }

            var hotkey = match.Groups[1].Value;
            var spacing = match.Groups[2].Value;
            var rest = match.Groups[3].Value;
            return $"<color=#CFC041FF>{hotkey}</color>{spacing}{rest}";
        }

        private static string Short(string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return "<empty>";
            }

            return value.Length > 120 ? value.Substring(0, 120) + "..." : value;
        }
    }
}
