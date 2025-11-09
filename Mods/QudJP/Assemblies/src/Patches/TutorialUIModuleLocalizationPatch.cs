using System.Collections.Generic;
using HarmonyLib;
using XRL.CharacterBuilds.Qud;

namespace QudJP.Patches
{
    /// <summary>
    /// ゲームモード／キャラタイプのタイトル・説明だけを辞書で置き換え、日本語表示を復元する。
    /// </summary>
    internal static class TutorialUIModuleLocalizationPatch
    {
        private sealed class TextPair
        {
            public string Title { get; }
            public string? Description { get; }

            public TextPair(string title, string? description = null)
            {
                Title = title;
                Description = description;
            }
        }

        private static readonly Dictionary<string, TextPair> GameModeText = new()
        {
            ["Tutorial"] = new("チュートリアル", "Caves of Qud の基本を学びます。"),
            ["Classic"] = new("クラシック", "パーマデス: 死亡するとキャラクターを失います。"),
            ["Roleplay"] = new("ロールプレイ", "集落でチェックポイントが有効になります。"),
            ["Wander"] = new(
                "放浪",
                "{{c|&#249;}} 多くのクリーチャーがあなたに中立で始まります。\n" +
                "{{c|&#249;}} 敵を倒しても経験値は得られません。\n" +
                "{{c|&#249;}} 発見や水儀を行うとより多くの経験値を得ます。\n" +
                "{{c|&#249;}} 集落でチェックポイントが有効になります。"),
            ["Daily"] = new(
                "デイリー",
                "{{c|&#249;}} 固定のキャラクターと世界シードで一度だけ挑戦できます。\n" +
                "{{c|&#249;}} 現在は {{W|{year}}} 年の {{W|{day_of_year}}} 日目です。")
        };

        private static readonly Dictionary<string, TextPair> ChartypeText = new()
        {
            ["Pregen"] = new("プリセット", "いくつかのプリセットキャラクターから選びます。慣れてきたら自由にカスタマイズできます。"),
            ["New"] = new("新規作成", "新しいキャラクターを作成します。"),
            ["Random"] = new("ランダム", "ランダムなキャラクターを生成します。"),
            ["Library"] = new("ライブラリ", "ビルドライブラリからキャラクターを選びます。"),
            ["Last"] = new("最後に遊んだキャラクター", "直前に遊んだキャラクターで再開します。")
        };

        [HarmonyPatch(typeof(QudGamemodeModule), nameof(QudGamemodeModule.HandleModesNode))]
        private static class GamemodeModulePatch
        {
            private static void Postfix(QudGamemodeModule __instance)
            {
                if (__instance?.GameModes == null)
                {
                    return;
                }

                foreach (var kvp in GameModeText)
                {
                    if (__instance.GameModes.TryGetValue(kvp.Key, out var descriptor))
                    {
                        descriptor.Title = kvp.Value.Title;
                        if (!string.IsNullOrEmpty(kvp.Value.Description))
                        {
                            descriptor.Description = kvp.Value.Description;
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(QudChartypeModule), nameof(QudChartypeModule.HandleTypesNode))]
        private static class ChartypeModulePatch
        {
            private static void Postfix(QudChartypeModule __instance)
            {
                if (__instance?.GameTypes == null)
                {
                    return;
                }

                foreach (var kvp in ChartypeText)
                {
                    if (__instance.GameTypes.TryGetValue(kvp.Key, out var descriptor))
                    {
                        descriptor.Title = kvp.Value.Title;
                        if (!string.IsNullOrEmpty(kvp.Value.Description))
                        {
                            descriptor.Description = kvp.Value.Description;
                        }
                    }
                }
            }
        }
    }
}
