using System;
using System.Collections.Generic;
using UnityEngine;
using XRL.UI;
using XRL.UI.Framework;

namespace QudJP.Localization
{
    internal static class MenuOptionLegendLocalizer
    {
        private static string Highlight(string text) => $"{{{{y|{text}}}}}";

        private static readonly Dictionary<string, string> CommandFallback = new()
        {
            ["NavigationXYAxis"] = Highlight("移動"),
            ["UI:Navigate"] = Highlight("移動"),
            ["UI:Navigate/up"] = Highlight("上へ移動"),
            ["UI:Navigate/down"] = Highlight("下へ移動"),
            ["UI:Navigate/left"] = Highlight("左へ移動"),
            ["UI:Navigate/right"] = Highlight("右へ移動"),
            ["Accept"] = Highlight("決定"),
            ["Cancel"] = Highlight("キャンセル"),
            ["Page Left"] = Highlight("前のページ"),
            ["Page Right"] = Highlight("次のページ"),
            ["UI:CategoryLeft"] = Highlight("前のカテゴリ"),
            ["UI:CategoryRight"] = Highlight("次のカテゴリ"),
        };

        private static readonly Dictionary<string, string> LiteralFallback =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["navigate"] = Highlight("移動"),
                ["select"] = Highlight("決定"),
                ["quit"] = Highlight("終了"),
                ["exit"] = Highlight("終了"),
                ["back"] = Highlight("戻る"),
                ["previous"] = Highlight("前へ"),
                ["prev"] = Highlight("前へ"),
                ["next"] = Highlight("次へ"),
                ["confirm"] = Highlight("確認"),
                ["Show Tooltip"] = Highlight("ツールチップ表示"),
            };
        private static readonly HashSet<string> Logged = new();

        internal static bool TryApply(MenuOption? option)
        {
            if (option == null)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(option.InputCommand) &&
                TryResolveCommand(option, option.InputCommand!))
            {
                return true;
            }

            if (TryLocalizeLiteral(option.Description, out var literalReplacement))
            {
                option.Description = literalReplacement;
                return true;
            }

            return false;
        }

        internal static bool TryLocalizeLiteral(string? literal, out string? localized)
        {
            localized = null;

            var key = literal?.Trim();
            if (string.IsNullOrEmpty(key))
            {
                return false;
            }

            if (LiteralFallback.TryGetValue(key!, out var replacement))
            {
                localized = replacement;
                LogApplied($"literal:{key}", replacement);
                return true;
            }

            return false;
        }

        private static bool TryResolveCommand(MenuOption option, string commandId)
        {
            if (CommandFallback.TryGetValue(commandId, out var fallback))
            {
                option.Description = fallback;
                LogApplied($"cmd:{commandId}", fallback);
                return true;
            }

            if (CommandBindingManager.CommandsByID != null &&
                CommandBindingManager.CommandsByID.TryGetValue(commandId, out var command) &&
                !string.IsNullOrEmpty(command?.DisplayText))
            {
                option.Description = command!.DisplayText;
                LogApplied($"cmd:{commandId}:game", command!.DisplayText);
                return true;
            }

            return false;
        }


        private static void LogApplied(string key, string value)
        {
            if (Logged.Add($"{key}->{value}"))
            {
                Debug.Log($"[QudJP] MenuOption localized: {key} -> {value}");
            }
        }
    }
}

