using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using QudJP.Localization;
using XRL.UI;

namespace QudJP.Patches
{
    /// <summary>
    /// Ensures world generation progress logs are localized before they are rendered by either UI.
    /// </summary>
    [HarmonyPatch(typeof(WorldCreationProgress))]
    internal static class WorldGenerationLocalizationPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(WorldCreationProgress.NextStep))]
        private static void LocalizeNextStep([HarmonyArgument(0)] ref string Text)
        {
            Text = LocalizeStep(Text, "XRL.UI.WorldCreationProgress.NextStep");
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(WorldCreationProgress.StepProgress))]
        private static void LocalizeStepProgress([HarmonyArgument(0)] ref string StepText)
        {
            StepText = LocalizeStep(StepText, "XRL.UI.WorldCreationProgress.StepProgress");
        }

        [HarmonyTranspiler]
        [HarmonyPatch(nameof(WorldCreationProgress.Draw))]
        private static IEnumerable<CodeInstruction> LocalizeDraw(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Ldstr && instruction.operand is string text)
                {
                    if (text == "[ Creating World ]")
                    {
                        yield return new CodeInstruction(OpCodes.Ldstr, text);
                        yield return new CodeInstruction(
                            OpCodes.Call,
                            AccessTools.Method(typeof(WorldGenerationLocalizationPatch), nameof(LocalizeHeader)));
                        continue;
                    }

                    if (text == " : &GComplete!")
                    {
                        yield return new CodeInstruction(OpCodes.Ldstr, text);
                        yield return new CodeInstruction(
                            OpCodes.Call,
                            AccessTools.Method(typeof(WorldGenerationLocalizationPatch), nameof(LocalizeCompleteLabel)));
                        continue;
                    }
                }

                yield return instruction;
            }
        }

        private static string LocalizeStep(string value, string contextId)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value ?? string.Empty;
            }

            return SafeStringTranslator.SafeTranslate(value, contextId);
        }

        private static string LocalizeHeader(string fallback)
        {
            return SafeStringTranslator.SafeTranslate(fallback, "XRL.UI.WorldCreationProgress.Header");
        }

        private static string LocalizeCompleteLabel(string fallback)
        {
            return SafeStringTranslator.SafeTranslate(fallback, "XRL.UI.WorldCreationProgress.Complete");
        }
    }
}
