using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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
            ["Cudgel (dazes on critical hit)"] = "棍棒（クリティカル時に朦朧付与）",
            ["Short Blades (causes bleeding on critical hit)"] = "短剣（クリティカル時に出血）",
            ["Axe (cleaves armor on critical hit)"] = "斧（クリティカル時に装甲切断）",
            ["Long Blades (can dismember)"] = "長剣（切断可能）",
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

        internal static string ApplySubHeader(string? text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text ?? string.Empty;
            }

            var parts = text!.Split(',');
            var output = new List<string>(parts.Length);
            foreach (var raw in parts)
            {
                var p = raw.Trim();
                var localized = ApplyFeeling(p) ?? ApplyDifficulty(p);
                if (string.IsNullOrEmpty(localized))
                {
                    localized = SafeStringTranslator.SafeTranslate(p, "Look.SubHeader");
                }
                output.Add(localized ?? p);
            }

            return string.Join("、 ", output);
        }

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

            // Handle color markup like "{{K|Label}}: value" by extracting the inner payload
            // and preserving any tail after the color tag (e.g., ": value").
            string prefix = string.Empty, suffix = string.Empty, payload = trimmed, tail = string.Empty;
            if (TryExtractOuterColorTag(trimmed, out var pfx, out var inner, out var after))
            {
                prefix = pfx;
                suffix = "}}";
                payload = inner;
                tail = after;
            }

            // Token-level quick replacements that appear embedded (e.g., in display names)
            if (payload.IndexOf("(unburnt)", StringComparison.Ordinal) >= 0)
            {
                var replacedToken = payload.Replace("(unburnt)", "（未点火）");
                if (!string.Equals(replacedToken, payload, StringComparison.Ordinal))
                {
                    var wrappedToken = prefix + replacedToken + suffix + tail;
                    return ReplaceTrimmed(line, trimmed, wrappedToken);
                }
            }

            // Pattern: "+1 to hit" -> "+1 命中"
            var mToHit = Regex.Match(payload, @"^[+\-]?\s*(?<n>\d+)\s+to\s+hit\s*$", RegexOptions.IgnoreCase);
            if (mToHit.Success)
            {
                var n = mToHit.Groups["n"].Value;
                var wrapped = prefix + $"+{n} 命中" + suffix + tail;
                return ReplaceTrimmed(line, trimmed, wrapped);
            }

            // Pattern: "On penetration, this weapon causes bleeding: X damage per round; save difficulty N."
            var mBleed = Regex.Match(payload, @"^On\s+penetration,\s*this\s+weapon\s+causes\s+bleeding:\s*(?<d>[^;]+)\s*damage\s+per\s+round;\s*save\s+difficulty\s*(?<sd>\d+)\.?$", RegexOptions.IgnoreCase);
            if (mBleed.Success)
            {
                var d = mBleed.Groups["d"].Value.Trim();
                var sd = mBleed.Groups["sd"].Value.Trim();
                var wrapped = prefix + $"貫通時、この武器は出血を与える：{d} ダメージ/ラウンド；セーブ難易度 {sd}" + suffix + tail;
                return ReplaceTrimmed(line, trimmed, wrapped);
            }

            var dictionaryLine = SafeStringTranslator.SafeTranslate(payload, "Look.TooltipLine");
            if (!string.Equals(dictionaryLine, payload, StringComparison.Ordinal))
            {
                // If the tail is a colon + value, honor Japanese colon and localize units where applicable.
                if (!string.IsNullOrEmpty(tail) && TryExtractColonTail(tail, out var leadWS2, out var valueAfterColon2))
                {
                    var value = valueAfterColon2;
                    if (string.Equals(payload, "Weight", StringComparison.Ordinal))
                    {
                        value = LocalizeValue("Weight", value);
                    }
                    var wrappedColon = prefix + dictionaryLine + suffix + leadWS2 + "：" + value;
                    return ReplaceTrimmed(line, trimmed, wrappedColon);
                }

                // Otherwise, preserve tail as-is.
                var wrapped = prefix + dictionaryLine + suffix + tail;
                return ReplaceTrimmed(line, trimmed, wrapped);
            }

            if (ExactRuleLineMap.TryGetValue(payload, out var exactReplacement))
            {
                var wrapped = prefix + exactReplacement + suffix + tail;
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
                    var wrapped = prefix + $"{translatedLabel}：{localizedValue}" + suffix + tail;
                    return ReplaceTrimmed(line, trimmed, wrapped);
                }

                if (label.EndsWith(" Bonus Cap", StringComparison.Ordinal))
                {
                    var stat = label.Substring(0, label.Length - " Bonus Cap".Length).Trim();
                    var localizedLabel = $"{SafeStringTranslator.SafeTranslate(stat, "Stat.Name")} ボーナス上限";
                    var wrapped = prefix + $"{localizedLabel}：{value}" + suffix + tail;
                    return ReplaceTrimmed(line, trimmed, wrapped);
                }

                // Already-localized label (e.g., "重量: 10 lbs.")
                if (string.Equals(label, "重量", StringComparison.Ordinal))
                {
                    var wrapped = prefix + $"重量：{LocalizeValue(label, value)}" + suffix + tail;
                    return ReplaceTrimmed(line, trimmed, wrapped);
                }
            }

            // Orphan value tail (no label): fix colon and localize units, e.g., ": 1 lbs." -> "：1 ポンド"
            if (payload.Length > 0 && payload[0] == ':')
            {
                var value = payload.Substring(1).TrimStart();
                if (value.IndexOf("lbs.", StringComparison.Ordinal) >= 0)
                {
                    value = value.Replace("lbs.", "ポンド");
                }
                var wrapped = prefix + "：" + value + suffix + tail;
                return ReplaceTrimmed(line, trimmed, wrapped);
            }

            // Case where label is inside color tag and colon/value are in the tail (e.g., "{{K|Weight}}: 1 lbs.")
            if (!string.IsNullOrEmpty(tail) && TryExtractColonTail(tail, out var leadWS, out var valueAfterColon))
            {
                var label = payload;
                var value = valueAfterColon;
                if (ColonLabelMap.TryGetValue(label, out var translatedLabel2))
                {
                    var wrapped = prefix + translatedLabel2 + suffix + leadWS + "：" + LocalizeValue(label, value);
                    return ReplaceTrimmed(line, trimmed, wrapped);
                }

                if (label.EndsWith(" Bonus Cap", StringComparison.Ordinal))
                {
                    var stat = label.Substring(0, label.Length - " Bonus Cap".Length).Trim();
                    var localizedLabel = $"{SafeStringTranslator.SafeTranslate(stat, "Stat.Name")} ボーナス上限";
                    var wrapped = prefix + localizedLabel + suffix + leadWS + "：" + value;
                    return ReplaceTrimmed(line, trimmed, wrapped);
                }
            }

            // Case where label is inside color tag and a numeric/percent tail follows without a colon
            // Example: "{{y|Critical hit chance}} 15%"
            if (!string.IsNullOrEmpty(tail) && !tail.StartsWith(":", StringComparison.Ordinal))
            {
                var tailTrim = tail.TrimStart();
                if (LooksLikeValueTail(tailTrim))
                {
                    var label = payload;
                    string localizedLabel = label;

                    if (ColonLabelMap.TryGetValue(label, out var mapped))
                    {
                        localizedLabel = mapped;
                    }
                    else if (label.EndsWith(" Bonus Cap", StringComparison.Ordinal))
                    {
                        var stat = label.Substring(0, label.Length - " Bonus Cap".Length).Trim();
                        localizedLabel = $"{SafeStringTranslator.SafeTranslate(stat, "Stat.Name")} ボーナス上限";
                    }
                    else
                    {
                        var fromDict = SafeStringTranslator.SafeTranslate(label, "Look.TooltipLine");
                        if (!string.Equals(fromDict, label, StringComparison.Ordinal))
                        {
                            localizedLabel = fromDict;
                        }
                        else
                        {
                            var fromLabel = SafeStringTranslator.SafeTranslate(label, "Look.TooltipLabel");
                            if (!string.Equals(fromLabel, label, StringComparison.Ordinal))
                            {
                                localizedLabel = fromLabel;
                            }
                        }
                    }

                    // Localize units inside the tail if applicable (e.g., lbs.) while preserving original spacing
                    var localizedTail = tail;
                    if (string.Equals(label, "Weight", StringComparison.Ordinal) && localizedTail.IndexOf("lbs.", StringComparison.Ordinal) >= 0)
                    {
                        localizedTail = localizedTail.Replace("lbs.", "ポンド");
                    }

                    var wrapped = prefix + localizedLabel + suffix + localizedTail;
                    return ReplaceTrimmed(line, trimmed, wrapped);
                }
            }

            const string ProjectilePrefix = "Projectiles fired with this weapon receive bonus penetration based on the wielder's ";
            if (payload.StartsWith(ProjectilePrefix, StringComparison.Ordinal))
            {
                var stat = payload.Substring(ProjectilePrefix.Length).TrimEnd('.');
                var localized = $"この武器の投射体は操者の{SafeStringTranslator.SafeTranslate(stat, "Stat.Name")}に応じて貫通力ボーナスを得る。";
                var wrapped = prefix + localized + suffix + tail;
                return ReplaceTrimmed(line, trimmed, wrapped);
            }

            const string FreezingPrefix = "Freezing: When powered, this weapon deals an additional ";
            const string FreezingSuffix = " cold damage on hit.";
            if (payload.StartsWith(FreezingPrefix, StringComparison.Ordinal) && payload.EndsWith(FreezingSuffix, StringComparison.Ordinal))
            {
                var middle = payload.Substring(FreezingPrefix.Length, payload.Length - FreezingPrefix.Length - FreezingSuffix.Length).Trim();
                var localized = $"凍結: 通電時、命中時に追加で {middle} 冷気ダメージを与える。";
                var wrapped = prefix + localized + suffix + tail;
                return ReplaceTrimmed(line, trimmed, wrapped);
            }

            if (TryLocalizeStatLine(payload, out var statLine))
            {
                var wrapped = prefix + statLine + suffix + tail;
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
            // Dictionary substitutions for common value payloads
            value = SafeStringTranslator.SafeTranslate(value, "Look.TooltipValue");

            // Unit normalization (be tolerant about the label and punctuation)
            if (value.IndexOf("lbs", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                value = value.Replace("lbs.", "ポンド").Replace("lbs", "ポンド");
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

        private static bool TryExtractOuterColorTag(string value, out string prefix, out string inner, out string after)
        {
            prefix = string.Empty;
            inner = value;
            after = string.Empty;
            if (!value.StartsWith("{{", StringComparison.Ordinal))
            {
                return false;
            }

            var pipe = value.IndexOf('|');
            if (pipe <= 2)
            {
                return false;
            }

            var close = value.IndexOf("}}", pipe + 1);
            if (close < 0)
            {
                return false;
            }

            prefix = value.Substring(0, pipe + 1);
            inner = value.Substring(pipe + 1, close - (pipe + 1));
            after = value.Substring(close + 2);
            return true;
        }

        private static bool TryExtractColonTail(string tail, out string leadingWhitespace, out string value)
        {
            leadingWhitespace = string.Empty;
            value = string.Empty;
            if (string.IsNullOrEmpty(tail))
            {
                return false;
            }

            var colonIndex = tail.IndexOf(':');
            if (colonIndex < 0)
            {
                return false;
            }

            // Only allow whitespace before ':' so we don't mis-handle other content.
            for (int i = 0; i < colonIndex; i++)
            {
                if (!char.IsWhiteSpace(tail[i]))
                {
                    return false;
                }
            }

            leadingWhitespace = colonIndex > 0 ? tail.Substring(0, colonIndex) : string.Empty;
            value = colonIndex + 1 < tail.Length ? tail.Substring(colonIndex + 1).TrimStart() : string.Empty;
            return true;
        }

        private static bool LooksLikeValueTail(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return false;
            }

            // Starts with +/-, digit, or open parenthesis followed soon by a digit; often ends with units or %
            if (s.Length > 0)
            {
                var c = s[0];
                if (char.IsDigit(c) || c == '+' || c == '-' || c == '(')
                {
                    return true;
                }
            }

            // Simple heuristic without regex to avoid compiler escape issues
            int idx = 0;
            while (idx < s.Length && char.IsWhiteSpace(s[idx])) idx++;
            if (idx >= s.Length) return false;
            var h = s[idx];
            if (char.IsDigit(h) || h == '+' || h == '-' || h == '(')
            {
                return true;
            }
            return false;
        }
    }
}

