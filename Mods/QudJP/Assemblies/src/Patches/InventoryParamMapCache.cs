using System;
using System.Collections.Generic;
using Qud.UI;
using QudJP.Localization;
using XRL.UI;

namespace QudJP.Patches
{
    /// <summary>
    /// Remembers the localized strings applied to InventoryLine instances so TMP_Text translation
    /// hooks can skip re-processing already translated values on the UI thread.
    /// </summary>
    internal static class InventoryParamMapCache
    {
        private const int MaxEntries = 32;
        private static readonly object Gate = new();
        private static readonly Dictionary<string, Dictionary<string, string>> Maps =
            new(StringComparer.OrdinalIgnoreCase);
        private static readonly LinkedList<string> Order = new();

        public static void Remember(string? eid, InventoryLineData? data)
        {
            if (string.IsNullOrEmpty(eid) || data == null)
            {
                return;
            }

            var snapshot = BuildSnapshot(data);
            if (snapshot.Count == 0)
            {
                return;
            }

            lock (Gate)
            {
                Maps[eid!] = snapshot;
                Order.AddLast(eid!);
                TrimExcess();
            }
        }

        public static bool IsLocalizedValue(string? contextId, string? eid, string? value)
        {
            if (string.IsNullOrEmpty(contextId) || string.IsNullOrEmpty(value))
            {
                return false;
            }

            Dictionary<string, string>? snapshot = null;
            lock (Gate)
            {
                if (!string.IsNullOrEmpty(eid) && Maps.TryGetValue(eid!, out var map))
                {
                    snapshot = map;
                }
                else
                {
                    snapshot = null;
                }
            }

            if (snapshot == null || snapshot.Count == 0)
            {
                return false;
            }

            var field = ExtractFieldName(contextId!);
            if (string.IsNullOrEmpty(field))
            {
                return false;
            }

            if (snapshot.TryGetValue(field, out var stored) &&
                string.Equals(stored, value, StringComparison.Ordinal))
            {
                return true;
            }

            return false;
        }

        private static Dictionary<string, string> BuildSnapshot(InventoryLineData data)
        {
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (data.category)
            {
                if (!string.IsNullOrEmpty(data.categoryName))
                {
                    map["CategoryLabel"] = data.categoryName!;
                }

                var weightText = InventoryLabelLocalizer.FormatCategoryWeight(
                    data.categoryAmount,
                    data.categoryWeight,
                    Options.ShowNumberOfItems);
                map["CategoryWeight"] = weightText;
            }
            else
            {
                var label = data.displayName ?? data.go?.DisplayName;
                if (!string.IsNullOrEmpty(label))
                {
                    map["Description"] = label!;
                }

                if (data.go != null)
                {
                    map["ItemWeight"] = InventoryLabelLocalizer.FormatItemWeight(data.go.Weight);
                }

            }

            return map;
        }

        private static string ExtractFieldName(string contextId)
        {
            var index = contextId.LastIndexOf('.');
            if (index < 0 || index >= contextId.Length - 1)
            {
                return contextId;
            }

            return contextId.Substring(index + 1);
        }

        private static void TrimExcess()
        {
            lock (Gate)
            {
                while (Order.Count > MaxEntries)
                {
                    var node = Order.First;
                    if (node == null)
                    {
                        break;
                    }

                    Order.RemoveFirst();
                    Maps.Remove(node.Value);
                }
            }
        }
    }
}
