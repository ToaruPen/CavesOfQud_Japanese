using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ModelShark;

namespace QudJP.Patches
{
    /// <summary>
    /// BodyText 系プレースホルダに対応する値を TooltipTrigger 単位で一時保存し、
    /// ParameterizedTextField から欠落している場合でもプレースホルダ復元で参照できるようにする。
    /// </summary>
    internal static class TooltipBodyTextCache
    {
        private static readonly ConditionalWeakTable<TooltipTrigger, FieldHolder> Store = new();

        public static void Remember(TooltipTrigger? trigger, string? field, string? value)
        {
            if (trigger == null || string.IsNullOrEmpty(field) || string.IsNullOrEmpty(value))
            {
                return;
            }

            if (!IsBodyTextField(field!))
            {
                return;
            }

            var holder = Store.GetValue(trigger, _ => new FieldHolder());
            holder.Fields[field!] = value!;
        }

        public static void MergeInto(Dictionary<string, string> snapshot, TooltipTrigger? trigger)
        {
            if (snapshot == null || trigger == null)
            {
                return;
            }

            if (!Store.TryGetValue(trigger, out var holder) || holder.Fields.Count == 0)
            {
                return;
            }

            foreach (var pair in holder.Fields)
            {
                if (snapshot.ContainsKey(pair.Key))
                {
                    continue;
                }

                snapshot[pair.Key] = pair.Value;
            }

            Store.Remove(trigger);
        }

        private static bool IsBodyTextField(string field) =>
            field.StartsWith("BodyText", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(field, "Body", StringComparison.OrdinalIgnoreCase);

        private sealed class FieldHolder
        {
            public Dictionary<string, string> Fields { get; } = new(StringComparer.OrdinalIgnoreCase);
        }
    }
}
