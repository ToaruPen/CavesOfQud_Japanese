using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

namespace QudJP.Patches
{
    internal static class TooltipPlaceholderRestorer
    {
        private static readonly Regex PlaceholderRegex = new("%(?<name>[A-Za-z0-9]+)%", RegexOptions.Compiled);

        public static bool TryRestoreText(TMP_Text text, Dictionary<string, string> values)
        {
            if (text == null || values == null || values.Count == 0)
            {
                return false;
            }

            var current = text.text;
            if (string.IsNullOrEmpty(current) || current.IndexOf('%') < 0)
            {
                return false;
            }

            var restored = Restore(current, values);
            if (string.Equals(restored, current, StringComparison.Ordinal))
            {
                return false;
            }

            text.text = restored;
            text.ForceMeshUpdate(ignoreActiveState: true, forceTextReparsing: true);
            Debug.Log($"[QudJP] Tooltip placeholder restored on '{text.gameObject?.name ?? "<null>"}'");
            return true;
        }

        public static bool TryRestoreString(ref string value, Dictionary<string, string> values)
        {
            if (string.IsNullOrEmpty(value) || value.IndexOf('%') < 0)
            {
                return false;
            }

            if (values == null || values.Count == 0)
            {
                return false;
            }

            var restored = Restore(value, values);
            if (string.Equals(restored, value, StringComparison.Ordinal))
            {
                return false;
            }

            value = restored;
            return true;
        }

        public static string Restore(string text, Dictionary<string, string> values)
        {
            if (string.IsNullOrEmpty(text) || values == null || values.Count == 0)
            {
                return text;
            }

            return PlaceholderRegex.Replace(
                text,
                match =>
                {
                    var key = match.Groups["name"].Value;
                    if (string.IsNullOrEmpty(key))
                    {
                        return match.Value;
                    }

                    foreach (var candidate in TooltipGuardHelper.CandidatesFromName(key))
                    {
                        if (!string.IsNullOrEmpty(candidate) &&
                            values.TryGetValue(candidate, out var value) &&
                            !string.IsNullOrWhiteSpace(value))
                        {
                            return value;
                        }
                    }

                    return match.Value;
                });
        }
    }

    internal static class TooltipParamMapCache
    {
        private static readonly object Gate = new();
        private static readonly Dictionary<string, Dictionary<string, string>> MapsByEid =
            new(StringComparer.OrdinalIgnoreCase);
        private static readonly LinkedList<string> Order = new();
        private static readonly Dictionary<string, LinkedListNode<string>> OrderLookup =
            new(StringComparer.OrdinalIgnoreCase);

        private const int MaxEntries = 16;
        private static Dictionary<string, string>? _lastMap;

        public static void Remember(string? eid, Dictionary<string, string> map)
        {
            if (map == null || map.Count == 0)
            {
                return;
            }

            var snapshot = new Dictionary<string, string>(map, StringComparer.OrdinalIgnoreCase);

            lock (Gate)
            {
                _lastMap = snapshot;
                if (string.IsNullOrEmpty(eid))
                {
                    return;
                }

                if (OrderLookup.TryGetValue(eid!, out var existing))
                {
                    Order.Remove(existing);
                    OrderLookup.Remove(eid!);
                }

                var node = Order.AddLast(eid!);
                OrderLookup[eid!] = node;
                MapsByEid[eid!] = snapshot;

                TrimExcess();
            }
        }

        public static bool TryRestorePlaceholders(ref string text, string? eid)
        {
            if (string.IsNullOrEmpty(text) || text.IndexOf('%') < 0)
            {
                return false;
            }

            var map = Resolve(eid);
            if (map == null)
            {
                return false;
            }

            return TooltipPlaceholderRestorer.TryRestoreString(ref text, map);
        }

        public static Dictionary<string, string>? Resolve(string? eid)
        {
            lock (Gate)
            {
                if (!string.IsNullOrEmpty(eid) && MapsByEid.TryGetValue(eid!, out var map))
                {
                    return map;
                }

                return _lastMap;
            }
        }

        private static void TrimExcess()
        {
            while (Order.Count > MaxEntries)
            {
                var node = Order.First;
                if (node == null)
                {
                    break;
                }

                Order.RemoveFirst();
                OrderLookup.Remove(node.Value);
                MapsByEid.Remove(node.Value);
            }
        }
    }
}
