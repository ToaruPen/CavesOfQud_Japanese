using HarmonyLib;
using QudJP.Diagnostics;
using QudJP.Localization;
using TMPro;

namespace QudJP.Patches
{
    [HarmonyPatch(typeof(TMP_Text))]
    internal static class TMPTextSetTextPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("set_text")]
        private static void BeforeSetText(TMP_Text __instance, ref string value, ref string __state)
        {
            var eid = UIContext.Resolve(__instance) ?? UIContext.Capture();
            __state = eid;

            var original = value ?? string.Empty;
            var sample = original.Length > 64 ? original.Substring(0, 64) + "..." : original;
            JpLog.Info(eid, "TMP", "set_text/IN", $"obj={__instance.gameObject?.name ?? __instance.GetType().Name} len={original.Length} sample='{sample.Replace("\n", "\\n")}'");

            var context = BuildContext(__instance);
            var translated = Translator.Instance.Apply(original, context);
            var normalized = TokenNormalizer.TryNormalize(translated);
            value = string.IsNullOrEmpty(normalized) ? translated : normalized;
        }

        [HarmonyPostfix]
        [HarmonyPatch("set_text")]
        private static void AfterSetText(TMP_Text __instance, string __state)
        {
            var eid = __state ?? UIContext.Resolve(__instance) ?? UIContext.Capture();
            var rendered = __instance.GetRenderedValues(true);
            var outText = __instance.text ?? string.Empty;
            JpLog.Info(
                eid,
                "TMP",
                "set_text/OUT",
                $"obj={__instance.gameObject?.name ?? __instance.GetType().Name} len={outText.Length} rendered=({rendered.x:F1},{rendered.y:F1}) font={__instance.font?.name ?? "<null>"} wrap={__instance.textWrappingMode} ovf={__instance.overflowMode}");

            if (string.IsNullOrEmpty(outText))
            {
                var stack = new System.Diagnostics.StackTrace(1, true);
                JpLog.Info(eid, "TMP", "set_text/STACK", stack.ToString());
            }

            UIContext.Release(eid);
        }

        private static string BuildContext(TMP_Text? instance)
        {
            var name = instance?.gameObject?.name;
            if (!string.IsNullOrEmpty(name))
            {
                return $"TMP.{name}";
            }

            var type = instance?.GetType().FullName;
            if (!string.IsNullOrEmpty(type))
            {
                return type!;
            }

            return "TMP_Text";
        }
    }
}
