using System;
using System.Collections.Generic;
using Qud.UI;

namespace QudJP.Localization
{
    internal static class TooltipTextLocalizer
    {
        private static readonly Dictionary<string, string> ColonLabelMap = new(StringComparer.Ordinal)
        {
            ["Gender"] = "性別",
            ["Physical features"] = "身体的特徴",
            ["Equipped"] = "装備品",
            ["Weight"] = "重量",
            ["Weapon Class"] = "武器カテゴリ",
            ["Accuracy"] = "命中精度",
            ["Multiple ammo used per shot"] = "1射で消費する弾薬",
            ["Multiple projectiles per shot"] = "1射で発射する弾数",
            ["Offhand Attack Chance"] = "サブアーム攻撃率",
        };

        private static readonly Dictionary<string, string> ExactRuleLineMap = new(StringComparer.Ordinal)
        {
            ["Spray fire: This item can be fired while adjacent to multiple enemies without risk of the shot going wild."] =
                "乱射: 複数の敵に隣接していても暴発せずに射撃できる。",
            ["-25 move speed"] = "-25 移動速度",
        };

        private static readonly Dictionary<string, string> StatLineMap = new(StringComparer.Ordinal)
        {
            ["Strength"] = "筋力",
            ["Agility"] = "敏捷",
            ["Toughness"] = "頑健",
            ["Intelligence"] = "知力",
            ["Willpower"] = "意志力",
            ["Ego"] = "自我",
        };

        private static readonly Dictionary<string, string> FeelingMap = new(StringComparer.Ordinal)
        {
            ["{{G|Friendly}}"] = "{{G|友好}}",
            ["{{R|Hostile}}"] = "{{R|敵対}}",
            ["Neutral"] = "中立",
        };

        private static readonly Dictionary<string, string> DifficultyMap = new(StringComparer.Ordinal)
        {
            ["{{R|Impossible}}"] = "{{R|不可能}}",
            ["{{r|Very Tough}}"] = "{{r|非常に困難}}",
            ["{{W|Tough}}"] = "{{W|難しい}}",
            ["{{w|Average}}"] = "{{w|普通}}",
            ["{{g|Easy}}"] = "{{g|易しい}}",
            ["{{G|Trivial}}"] = "{{G|造作ない}}",
        };

        private static readonly Dictionary<string, string> WoundMap = new(StringComparer.Ordinal)
        {
            ["{{r|Badly Wounded}}"] = "{{r|重傷}}",
            ["{{R|Wounded}}"] = "{{R|負傷}}",
            ["{{W|Injured}}"] = "{{W|軽傷}}",
            ["{{G|Fine}}"] = "{{G|良好}}",
            ["{{Y|Perfect}}"] = "{{Y|無傷}}",
            ["{{r|Badly Damaged}}"] = "{{r|大破}}",
            ["{{R|Damaged}}"] = "{{R|損傷}}",
            ["{{W|Lightly Damaged}}"] = "{{W|小破}}",
        };

        internal static string ApplyLongDescription(string? text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text ?? string.Empty;
            }

            var normalized = text!.Replace("\r\n", "\n").Replace("\r", "\n");
            var segments = normalized.Split('\n');
            for (var i = 0; i < segments.Length; i++)
            {
                segments[i] = LocalizeLine(segments[i]);
            }

            return string.Join("\n", segments);
        }

        internal static string? ApplyFeeling(string? token) => TranslateExactToken(token, FeelingMap);

        internal static string? ApplyDifficulty(string? token) => TranslateExactToken(token, DifficultyMap);

        internal static string? ApplyWoundLevel(string? token) => TranslateExactToken(token, WoundMap);

        private static string? TranslateExactToken(string? value, IReadOnlyDictionary<string, string> map)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            return map.TryGetValue(value!, out var localized) ? localized : value;
        }

        private static string LocalizeLine(string line)
        {
            var trimmed = line.Trim();
            if (trimmed.Length == 0 ||
                string.Equals(trimmed, "{{rules|", StringComparison.Ordinal) ||
                string.Equals(trimmed, "}}", StringComparison.Ordinal))
            {
                return line;
            }

            // Handle outer color markup like "{{K|...}}" by working on the inner payload
            // and re-wrapping with the same tag after translation.
            string prefix = string.Empty, suffix = string.Empty, payload = trimmed;
            if (TryStripOuterColorTag(trimmed, out var pfx, out var inner))
            {
                prefix = pfx;
                suffix = "}}";
                payload = inner;
            }

            var dictionaryLine = Translator.Instance.Apply(payload, "Look.TooltipLine");
            if (!string.Equals(dictionaryLine, payload, StringComparison.Ordinal))
            {
                var wrapped = prefix + dictionaryLine + suffix;
                return ReplaceTrimmed(line, trimmed, wrapped);
            }

            if (ExactRuleLineMap.TryGetValue(payload, out var exactReplacement))
            {
                var wrapped = prefix + exactReplacement + suffix;
                return ReplaceTrimmed(line, trimmed, wrapped);
            }

            var colonIndex = payload.IndexOf(':');
            if (colonIndex > 0)
            {
                var label = payload.Substring(0, colonIndex);
                var value = payload.Substring(colonIndex + 1).TrimStart();

                if (ColonLabelMap.TryGetValue(label, out var translatedLabel))
                {
                    var localizedValue = LocalizeValue(label, value);
                    var wrapped = prefix + $"{translatedLabel}：{localizedValue}" + suffix;
                    return ReplaceTrimmed(line, trimmed, wrapped);
                }

                if (label.EndsWith(" Bonus Cap", StringComparison.Ordinal))
                {
                    var stat = label.Substring(0, label.Length - " Bonus Cap".Length).Trim();
                    var localizedLabel = $"{Translator.Instance.Apply(stat, "Stat.Name")} ボーナス上限";
                    var wrapped = prefix + $"{localizedLabel}：{value}" + suffix;
                    return ReplaceTrimmed(line, trimmed, wrapped);
                }
            }

            const string ProjectilePrefix = "Projectiles fired with this weapon receive bonus penetration based on the wielder's ";
            if (payload.StartsWith(ProjectilePrefix, StringComparison.Ordinal))
            {
                var stat = payload.Substring(ProjectilePrefix.Length).TrimEnd('.');
                var localized = $"この武器の投射体は操者の{Translator.Instance.Apply(stat, "Stat.Name")}に応じて貫通力ボーナスを得る。";
                var wrapped = prefix + localized + suffix;
                return ReplaceTrimmed(line, trimmed, wrapped);
            }

            if (TryLocalizeStatLine(payload, out var statLine))
            {
                var wrapped = prefix + statLine + suffix;
                return ReplaceTrimmed(line, trimmed, wrapped);
            }

            return line;
        }

        private static bool TryLocalizeStatLine(string trimmed, out string localized)
        {
            foreach (var entry in StatLineMap)
            {
                var key = entry.Key;
                if (!trimmed.StartsWith(key, StringComparison.Ordinal))
                {
                    continue;
                }

                if (trimmed.Length > key.Length && !char.IsWhiteSpace(trimmed[key.Length]))
                {
                    continue;
                }

                var remainder = trimmed.Length > key.Length ? trimmed.Substring(key.Length) : string.Empty;
                localized = entry.Value + remainder;
                return true;
            }

            localized = string.Empty;
            return false;
        }

        private static string LocalizeValue(string label, string value)
        {
            if (string.Equals(label, "Weight", StringComparison.Ordinal) &&
                value.EndsWith("lbs.", StringComparison.Ordinal))
            {
                return value.Replace("lbs.", "ポンド");
            }

            return value;
        }

        private static string ReplaceTrimmed(string original, string trimmed, string replacement)
        {
            var index = original.IndexOf(trimmed, StringComparison.Ordinal);
            if (index < 0)
            {
                return replacement;
            }

            return original.Substring(0, index) + replacement + original.Substring(index + trimmed.Length);
        }

        private static bool TryStripOuterColorTag(string value, out string prefix, out string inner)
        {
            prefix = string.Empty;
            inner = value;
            if (value.Length >= 4 && value.StartsWith("{{") && value.EndsWith("}}"))
            {
                var pipe = value.IndexOf('|');
                if (pipe > 2)
                {
                    prefix = value.Substring(0, pipe + 1);
                    inner = value.Substring(pipe + 1, value.Length - (pipe + 1) - 2);
                    return true;
                }
            }

            return false;
        }
    }
}


