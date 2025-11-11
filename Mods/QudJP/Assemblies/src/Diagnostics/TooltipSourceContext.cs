using System.Runtime.CompilerServices;
using System.Text;
using ModelShark;
using Qud.UI;
using XRL.World;

namespace QudJP.Diagnostics
{
    /// <summary>
    /// Captures contextual information about tooltip sources (e.g., which inventory line
    /// spawned the tooltip) so downstream logging can describe the affected item.
    /// </summary>
    internal static class TooltipSourceContext
    {
        private static readonly ConditionalWeakTable<TooltipTrigger, SummaryHolder> Summaries = new();

        public static void Record(TooltipTrigger? trigger, BaseLineWithTooltip? host, GameObject? go, GameObject? compareGo)
        {
            if (trigger == null)
            {
                return;
            }

            var summary = BuildSummary(host, go, compareGo);
            if (string.IsNullOrEmpty(summary))
            {
                return;
            }

            if (Summaries.TryGetValue(trigger, out _))
            {
                Summaries.Remove(trigger);
            }

            Summaries.Add(trigger, new SummaryHolder(summary!, go, compareGo));
        }

        public static string? Describe(TooltipTrigger? trigger)
        {
            if (trigger != null && Summaries.TryGetValue(trigger, out var holder))
            {
                return holder.Summary;
            }

            return null;
        }

        public static bool TryGetSubjects(TooltipTrigger? trigger, out GameObject? primary, out GameObject? compare)
        {
            if (trigger != null && Summaries.TryGetValue(trigger, out var holder))
            {
                primary = holder.Primary;
                compare = holder.Compare;
                return true;
            }

            primary = null;
            compare = null;
            return false;
        }

        private static string? BuildSummary(BaseLineWithTooltip? host, GameObject? go, GameObject? compareGo)
        {
            var ownerType = host?.GetType().Name ?? "<unknown>";
            var hostName = host?.gameObject?.name ?? "<none>";

            InventoryLineData? inventoryLine = null;
            if (host is InventoryLine line)
            {
                inventoryLine = TryGetInventoryLineData(line);
            }

            // Prefer the line's GameObject reference since StartTooltip may null out the parameter.
            var primaryGo = inventoryLine?.go ?? go;

            var builder = new StringBuilder();
            builder.Append($"owner={ownerType}");
            builder.Append($" host='{Short(hostName)}'");

            if (inventoryLine != null)
            {
                var kind = inventoryLine.category ? "Category" : "Item";
                builder.Append($" kind={kind}");
                var label = inventoryLine.category ? inventoryLine.categoryName : inventoryLine.displayName;
                if (!string.IsNullOrWhiteSpace(label))
                {
                    builder.Append($" inv='{Short(StripMarkup(label))}'");
                }
            }

            if (primaryGo != null)
            {
                builder.Append($" go={DescribeGameObject(primaryGo)}");
            }

            if (compareGo != null)
            {
                builder.Append($" compare={DescribeGameObject(compareGo)}");
            }

            return builder.ToString();
        }

        private static string DescribeGameObject(GameObject go)
        {
            var display = go.DisplayName ?? go.Render?.DisplayName ?? go.Blueprint ?? go.DebugName ?? "<unnamed>";
            var blueprint = go.Blueprint ?? "<null>";
            var id = go.IDIfAssigned ?? "<none>";
            return $"'{Short(StripMarkup(display))}'[{blueprint}]#{id}";
        }

        private static InventoryLineData? TryGetInventoryLineData(InventoryLine line)
        {
            try
            {
                if (line.GetNavigationContext() is InventoryLine.Context ctx)
                {
                    return ctx.data;
                }
            }
            catch
            {
                // Ignore invalid casts or missing navigation contexts.
            }

            return null;
        }

        private static string Short(string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return "<empty>";
            }

            var nonNull = value!;
            var trimmed = nonNull.Replace('\n', ' ').Trim();
            return trimmed.Length > 80 ? trimmed.Substring(0, 80) + "..." : trimmed;
        }

        private static string StripMarkup(string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            var result = value!;
            // Remove {{color|...}} style markup.
            result = System.Text.RegularExpressions.Regex.Replace(result, "\\{\\{[^|{}]+\\|", string.Empty);
            result = result.Replace("{{", string.Empty).Replace("}}", string.Empty);
            return result;
        }

        private sealed class SummaryHolder
        {
            public SummaryHolder(string summary, GameObject? primary, GameObject? compare)
            {
                Summary = summary;
                Primary = primary;
                Compare = compare;
            }

            public string Summary { get; }
            public GameObject? Primary { get; }
            public GameObject? Compare { get; }
        }
    }
}
