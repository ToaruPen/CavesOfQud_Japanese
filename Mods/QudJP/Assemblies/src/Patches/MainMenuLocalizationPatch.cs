using HarmonyLib;
using Qud.UI;
using System.Collections.Generic;
using XRL.UI;
using XRL.UI.Framework;

namespace QudJP
{
    /// <summary>
    /// メインメニューのラベルを日本語へ差し替える。
    /// MainMenu の静的初期化後にオプションリストへ反映する形で対応。
    /// </summary>
    [HarmonyPatch(typeof(MainMenu), nameof(MainMenu.Show))]
    internal static class MainMenuLocalizationPatch
    {
        private static readonly Dictionary<string, string> CommandToText = new()
        {
            ["Pick:New Game"] = "新規ゲーム",
            ["Pick:Continue"] = "続きから",
            ["Pick:High Scores"] = "記録",
            ["Pick:Options"] = "オプション",
            ["Pick:Installed Mod Configuration"] = "Mod 管理",
            ["Pick:Redeem Code"] = "コード入力",
            ["Pick:Modding Utilities"] = "Mod ツール",
            ["Pick:Credits"] = "スタッフロール",
            ["Pick:Help"] = "ヘルプ",
        };

        private static void Prefix()
        {
            Apply(MainMenu.LeftOptions);
            Apply(MainMenu.RightOptions);
        }

        private static void Apply(List<MainMenuOptionData>? options)
        {
            if (options == null)
            {
                return;
            }

            foreach (var option in options)
            {
                if (option == null || string.IsNullOrEmpty(option.Command))
                {
                    continue;
                }

                if (CommandToText.TryGetValue(option.Command, out var text))
                {
                    option.Text = text;
                }
            }
        }
    }
}
