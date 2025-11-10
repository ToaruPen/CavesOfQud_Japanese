using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using XRL.Core;

namespace QudJP
{
    /// <summary>
    /// Localizes the system menu / quit confirmation strings that are hardcoded in XRLCore.PlayerTurn.
    /// </summary>
    [HarmonyPatch(typeof(XRLCore), nameof(XRLCore.PlayerTurn))]
    internal static class PlayerTurnLocalizationPatch
    {
        private static readonly Dictionary<string, string> StringReplacements = new()
        {
            ["&KSet Checkpoint"] = "&Kチェックポイントを設定",
            ["Set Checkpoint"] = "チェックポイントを設定",
            ["Restore Checkpoint"] = "チェックポイントを復元",
            ["&KRestore Checkpoint"] = "&Kチェックポイントを復元",
            ["Control Mapping"] = "操作設定",
            ["Options"] = "オプション",
            ["Game Info"] = "ゲーム情報",
            ["Save and Quit"] = "セーブして終了",
            ["Quit Without Saving"] = "セーブせずに終了",
            ["Abandon Character"] = "キャラクターを放棄",
            ["If you quit without saving, you will lose all your unsaved progress. Are you sure you want to QUIT and LOSE YOUR PROGRESS?\n\n Type '"] =
                "セーブせずに終了すると保存されていない進行状況がすべて失われます。本当に終了してよろしいですか？\n\n「",
            ["If you quit without saving, you will lose all your progress and your character will be lost. Are you sure you want to QUIT and LOSE YOUR PROGRESS?\n\nType '"] =
                "セーブせずに終了すると進行状況とキャラクターが完全に失われます。本当に終了してよろしいですか？\n\n「",
            ["' to confirm."] = "」と入力すると確定します。",
            ["You can only set your checkpoint in settlements."] = "集落内でのみチェックポイントを設定できます。",
            ["You can only restore your checkpoint outside settlements."] = "集落の外でのみチェックポイントを復元できます。",
            ["Are you sure you want to restore your checkpoint?"] = "本当にチェックポイントを復元しますか？"
        };

        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> Transpile(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Ldstr &&
                    instruction.operand is string value &&
                    StringReplacements.TryGetValue(value, out var replacement))
                {
                    instruction.operand = replacement;
                }

                yield return instruction;
            }
        }
    }
}
