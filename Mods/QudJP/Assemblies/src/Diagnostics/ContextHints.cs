using System.Runtime.CompilerServices;

namespace QudJP.Diagnostics
{
    /// <summary>
    /// Stores optional context identifiers for runtime components (e.g. TMP_Text) so that
    /// downstream translation can emit stable dictionary keys even when GameObject names
    /// are obfuscated or duplicated.
    /// </summary>
    internal static class ContextHints
    {
        private static readonly ConditionalWeakTable<object, HintHolder> Hints = new();

        public static void Set(object? target, string? contextId)
        {
            if (target == null || string.IsNullOrEmpty(contextId))
            {
                return;
            }

            if (Hints.TryGetValue(target, out _))
            {
                Hints.Remove(target);
            }

            Hints.Add(target, new HintHolder(contextId!));
        }

        public static string? Resolve(object? target)
        {
            if (target != null && Hints.TryGetValue(target, out var holder))
            {
                return holder.ContextId;
            }

            return null;
        }

        private sealed class HintHolder
        {
            public HintHolder(string contextId) => ContextId = contextId;
            public string ContextId { get; }
        }
    }
}
