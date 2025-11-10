using System;
using System.Collections.Concurrent;
using UnityEngine;

namespace QudJP.Diagnostics
{
    internal static class JpLog
    {
        private const int MaxPerMessage = 50;
        private static readonly ConcurrentDictionary<int, int> Counts = new();
        private static readonly string EnabledEnv = Environment.GetEnvironmentVariable("QUDJP_VERBOSE_LOG") ?? string.Empty;

        public static bool Enabled { get; set; } =
            !string.Equals(EnabledEnv, "0", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(EnabledEnv, "false", StringComparison.OrdinalIgnoreCase);

        public static string NewEID()
        {
            return Guid.NewGuid().ToString("N").Substring(0, 8);
        }

        public static void Info(string? eid, string category, string stage, string message)
        {
            if (!Enabled)
            {
                return;
            }

            var key = Combine(category, stage, message);
            var count = Counts.AddOrUpdate(key, 1, (_, current) => current + 1);
            if (count > MaxPerMessage)
            {
                if (count == MaxPerMessage + 1)
                {
                    Debug.Log($"[JP][{category}][{stage}][EID:{eid ?? "--------"}] (suppressed further repeats)");
                }

                return;
            }

            Debug.Log($"[JP][{category}][{stage}][EID:{eid ?? "--------"}] {message}");
        }

        private static int Combine(string category, string stage, string message)
        {
            unchecked
            {
                var hash = 17;
                hash = hash * 31 + (category?.GetHashCode() ?? 0);
                hash = hash * 31 + (stage?.GetHashCode() ?? 0);
                hash = hash * 31 + (message?.GetHashCode() ?? 0);
                return hash;
            }
        }
    }
}
