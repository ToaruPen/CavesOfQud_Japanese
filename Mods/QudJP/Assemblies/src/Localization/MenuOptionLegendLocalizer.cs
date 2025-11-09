using System;
using System.Collections.Generic;
using UnityEngine;
using XRL.UI;
using XRL.UI.Framework;

namespace QudJP.Localization
{
    internal static class MenuOptionLegendLocalizer
    {
        private static readonly Dictionary<string, string> CommandFallback = new()
        {
            ["NavigationXYAxis"] = "移動",
            ["UI:Navigate"] = "移動",
            ["UI:Navigate/up"] = "上へ移動",
            ["UI:Navigate/down"] = "下へ移動",
            ["UI:Navigate/left"] = "左へ移動",
            ["UI:Navigate/right"] = "右へ移動",
            ["Accept"] = "決定",
            ["Cancel"] = "戻る",
            ["Page Left"] = "前のページ",
            ["Page Right"] = "次のページ",
            ["UI:CategoryLeft"] = "前のカテゴリ",
            ["UI:CategoryRight"] = "次のカテゴリ",
        };

        private static readonly Dictionary<string, string> LiteralFallback =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["navigate"] = "移動",
                ["select"] = "決定",
                ["quit"] = "終了",
                ["exit"] = "終了",
                ["back"] = "戻る",
                ["previous"] = "前へ",
                ["prev"] = "前へ",
                ["next"] = "次へ",
                ["confirm"] = "決定",
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
