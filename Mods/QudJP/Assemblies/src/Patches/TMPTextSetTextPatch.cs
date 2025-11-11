using HarmonyLib;
using ModelShark;
using Qud.UI;
using QudJP;
using QudJP.Diagnostics;
using QudJP.Localization;
using TMPro;
using UnityEngine;

namespace QudJP.Patches
{
    [HarmonyPatch(typeof(TMP_Text))]
    internal static class TMPTextSetTextPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("set_text")]
        private static void BeforeSetText(TMP_Text __instance, ref string value, ref string __state)
        {
            FontManager.Instance.ApplyToText(__instance);
            var resolvedEid = UIContext.Resolve(__instance);
            var eid = resolvedEid ?? UIContext.Capture();
            __state = eid;

            var original = value ?? string.Empty;
            var sample = original.Length > 64 ? original.Substring(0, 64) + "..." : original;
            var context = BuildContext(__instance);
            var objName = __instance?.gameObject?.name ?? __instance?.GetType().Name ?? "TMP_Text";

            JpLog.Info(
                eid,
                "TMP",
                "set_text/IN",
                $"ctx={context} obj={objName} len={original.Length} sample='{sample.Replace("\n", "\\n")}'");

            string output;
            if (TranslationContextGuards.ShouldSkipTranslation(context, resolvedEid, original))
            {
                output = original;
            }
            else
            {
                var translated = Translator.Instance.Apply(original, context);
                var normalized = TokenNormalizer.TryNormalize(translated);
                output = string.IsNullOrEmpty(normalized) ? translated : normalized;
            }

            if (TooltipParamMapCache.TryRestorePlaceholders(ref output, resolvedEid))
            {
                var restoredSample = output.Length > 64 ? output.Substring(0, 64) + "..." : output;
                JpLog.Info(
                    eid,
                    "TMP",
                    "placeholder/RESTORE",
                    $"ctx={context} obj={objName} len={output.Length} sample='{restoredSample.Replace("\n", "\\n")}'");
            }

            value = output;
        }

        [HarmonyPostfix]
        [HarmonyPatch("set_text")]
        private static void AfterSetText(TMP_Text __instance, string __state)
        {
            var eid = __state ?? UIContext.Resolve(__instance) ?? UIContext.Capture();
            var context = BuildContext(__instance);
            var rendered = __instance.GetRenderedValues(true);
            var outText = __instance.text ?? string.Empty;
            JpLog.Info(
                eid,
                "TMP",
                "set_text/OUT",
                $"ctx={context} obj={__instance.gameObject?.name ?? __instance.GetType().Name} len={outText.Length} rendered=({rendered.x:F1},{rendered.y:F1}) font={__instance.font?.name ?? "<null>"} wrap={__instance.textWrappingMode} ovf={__instance.overflowMode}");

            if (string.IsNullOrEmpty(outText))
            {
                var stack = new System.Diagnostics.StackTrace(1, true);
                JpLog.Info(eid, "TMP", "set_text/STACK", stack.ToString());
            }

            UIContext.Release(eid);
        }

        private static string BuildContext(TMP_Text? instance)
        {
            var hinted = ContextHints.Resolve(instance);
            if (!string.IsNullOrEmpty(hinted))
            {
                return hinted!;
            }

            if (instance == null)
            {
                return "TMP_Text";
            }

            string BuildFromGameObject(string prefix)
            {
                var goName = instance.gameObject?.name ?? "Field";
                return $"{prefix}.{goName}";
            }

            var style = instance.GetComponentInParent<TooltipStyle>();
            if (style != null)
            {
                return $"ModelShark.Tooltip.{style.name ?? "Style"}.{instance.gameObject?.name ?? "Field"}";
            }

            if (instance.GetComponentInParent<PopupMessage>() != null)
            {
                return BuildFromGameObject("TMP.PopupMessage");
            }

            if (instance.GetComponentInParent<SelectableTextMenuItem>() != null)
            {
                return BuildFromGameObject("TMP.SelectableTextMenuItem");
            }

            if (instance.GetComponentInParent<InventoryLine>() != null)
            {
                return BuildFromGameObject("TMP.InventoryLine");
            }

            var name = instance.gameObject?.name;
            if (!string.IsNullOrEmpty(name))
            {
                return $"TMP.{name}";
            }

            var type = instance.GetType().FullName;
            return string.IsNullOrEmpty(type) ? "TMP_Text" : type!;
        }
    }
}
