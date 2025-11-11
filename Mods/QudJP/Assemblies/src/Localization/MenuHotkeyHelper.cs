using System;
using XRL.UI;

namespace QudJP.Localization
{
    internal static class MenuHotkeyHelper
    {
        public static string? GetPrimaryLabel(string? command, string? hotkey)
        {
            string? commandLabel = null;
            if (TryGetLabel(command, out var label, out var echoed))
            {
                commandLabel = label;
                if (!echoed)
                {
                    return label;
                }
            }

            if (TryGetLabel(hotkey, out label, out _))
            {
                return label;
            }

            return string.IsNullOrWhiteSpace(commandLabel) ? null : commandLabel;
        }

        private static bool TryGetLabel(string? descriptor, out string? label, out bool echoedDescriptor)
        {
            label = null;
            echoedDescriptor = false;
            if (string.IsNullOrWhiteSpace(descriptor))
            {
                return false;
            }

            string? fallback = null;
            var normalized = descriptor!;
            var commands = normalized.Split(new[] { ',', '|' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var cmd in commands)
            {
                var trimmed = cmd.Trim();
                if (string.IsNullOrEmpty(trimmed))
                {
                    continue;
                }

                var candidate = NormalizeDescriptor(trimmed);
                fallback ??= candidate;

                try
                {
                    var description = ControlManager.getCommandInputDescription(trimmed, Options.ModernUI);
                    if (!string.IsNullOrWhiteSpace(description))
                    {
                        label = description.Trim();
                        echoedDescriptor = string.Equals(label, trimmed, StringComparison.OrdinalIgnoreCase);
                        if (!echoedDescriptor)
                        {
                            return true;
                        }
                        // If the control manager just echoed the descriptor (common for literal chars),
                        // continue searching to see if another token yields an actual binding.
                        continue;
                    }
                }
                catch
                {
                    // Ignore lookup failures and try the next entry.
                }
            }

            if (!string.IsNullOrWhiteSpace(fallback))
            {
                label = fallback;
                echoedDescriptor = true;
                return true;
            }

            return false;
        }

        private static string NormalizeDescriptor(string value)
        {
            if (value.StartsWith("char:", StringComparison.OrdinalIgnoreCase) && value.Length > 5)
            {
                return value.Substring(5);
            }

            return value;
        }
    }
}
