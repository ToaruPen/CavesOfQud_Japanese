using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using JoppaTutorial;

namespace QudJP.Patches
{
    /// <summary>
    /// Replaces tutorial-specific hardcoded English strings with Japanese equivalents while preserving vanilla flow.
    /// </summary>
    [HarmonyPatch]
    public static class TutorialStringLocalizationPatch
    {
        private static readonly Dictionary<string, string> LateUpdateStrings = new()
        {
            {
                "Welcome to the Caves of Qud tutorial. We'll just be scratching the surface here, learning enough of the basics to help you get your footing.\n\nIn Caves of Qud, you play as a mutated human or true kin.\n\nFor the tutorial, we're picking mutated human.",
                "『Caves of Qud』のチュートリアルへようこそ。ここではごく基本だけをなぞり、歩き出すのに必要な土台を身に付けます。\n\nこのゲームでは突然変異した人間か、真の人間を操作します。\n\nチュートリアルでは突然変異した人間を選びましょう。"
            },
            {
                "Character creation is a deep and sometimes long process. We included some preset builds to help you get started. After the tutorial, you can try another build, or make a character from scratch. (The recommended way! Once you get your footing.) \n\nFor now, pick the marsh taur.",
                "キャラクター作成は奥深く、ときには時間のかかる工程です。すぐに遊べるよう、いくつかプリセットを用意しています。チュートリアルが終わったら、別のビルドを試したり、一から作ってみましょう（慣れてきたら特におすすめです）。\n\n今はマーシュ・ターを選んでください。"
            },
            {
                "Here is a summary of your attributes and mutations, which grant your character unique abilities.",
                "ここには能力値と突然変異の概要が表示されます。どちらもキャラクター固有の力を与えてくれます。"
            },
            {
                "You can name your character or choose Next for a random name.",
                "ここでキャラクターの名前を入力するか、［Next］でランダムな名前を採用できます。"
            }
        };

        private static readonly Dictionary<string, string> OnBootGameStrings = new()
        {
            {
                "On the 30th of Kisu Ux, you scuttle down a dark shaft at the edge of a sunken trade path and arrive at a caravanserai. It's powdered in salt and dust from across the ribbon of time.",
                "キス・ウックスの30日、あなたは沈んだ交易路の縁に穿たれた暗い縦穴を滑り降り、隊商宿へたどり着いた。そこは時間の川向こうから舞い込んだ塩と砂埃にまみれている。"
            }
        };

        [HarmonyPatch(typeof(IntroTutorialStart), nameof(IntroTutorialStart.LateUpdate))]
        private static class IntroTutorialStartLateUpdatePatch
        {
            private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                return ReplaceLdstr(instructions, LateUpdateStrings);
            }
        }

        [HarmonyPatch(typeof(IntroTutorialStart), nameof(IntroTutorialStart.OnBootGame))]
        private static class IntroTutorialStartOnBootGamePatch
        {
            private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                return ReplaceLdstr(instructions, OnBootGameStrings);
            }
        }

        private static IEnumerable<CodeInstruction> ReplaceLdstr(IEnumerable<CodeInstruction> instructions, IReadOnlyDictionary<string, string> replacements)
        {
            foreach (var instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Ldstr && instruction.operand is string value && replacements.TryGetValue(value, out var translated))
                {
                    yield return new CodeInstruction(OpCodes.Ldstr, translated);
                }
                else
                {
                    yield return instruction;
                }
            }
        }
    }
}
