using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using ModelShark;

namespace QudJP.Patches
{
    internal static class TooltipTraversal
    {
        internal static IEnumerable<Tooltip> EnumerateAll(TooltipTrigger trigger)
        {
            if (trigger == null)
            {
                yield break;
            }

            var visitedOwners = new HashSet<object>();
            var yielded = new HashSet<Tooltip>();

            foreach (var tooltip in EnumerateFromObject(trigger, visitedOwners))
            {
                if (tooltip != null && yielded.Add(tooltip))
                {
                    yield return tooltip;
                }
            }
        }

        internal static string? ResolveStyleName(Tooltip tooltip)
        {
            if (tooltip == null)
            {
                return null;
            }

            try
            {
                var type = tooltip.GetType();
                var field = type.GetField("tooltipStyle", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (field?.GetValue(tooltip) is TooltipStyle style && !string.IsNullOrEmpty(style.name))
                {
                    return style.name;
                }

                var property = type.GetProperty("tooltipStyle", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) ??
                    type.GetProperty("TooltipStyle", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (property?.GetValue(tooltip) is TooltipStyle propStyle && !string.IsNullOrEmpty(propStyle.name))
                {
                    return propStyle.name;
                }
            }
            catch
            {
                // Ignore reflection failures.
            }

            return null;
        }

        private static IEnumerable<Tooltip> EnumerateFromObject(object source, HashSet<object> visitedOwners)
        {
            if (source == null)
            {
                yield break;
            }

            if (source is Tooltip tooltip)
            {
                yield return tooltip;
                yield break;
            }

            if (source is TooltipTrigger trigger)
            {
                if (!visitedOwners.Add(trigger))
                {
                    yield break;
                }

                if (trigger.Tooltip != null)
                {
                    yield return trigger.Tooltip;
                }

                foreach (var nested in EnumerateFields(trigger, visitedOwners))
                {
                    yield return nested;
                }

                yield break;
            }

            if (source is IEnumerable enumerable && source is not string)
            {
                foreach (var element in enumerable)
                {
                    foreach (var nested in EnumerateFromObject(element, visitedOwners))
                    {
                        yield return nested;
                    }
                }

                yield break;
            }
        }

        private static IEnumerable<Tooltip> EnumerateFields(object owner, HashSet<object> visitedOwners)
        {
            if (owner == null)
            {
                yield break;
            }

            var type = owner.GetType();
            while (type != null && type != typeof(object))
            {
                foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
                {
                    object value;
                    try
                    {
                        value = field.GetValue(owner);
                    }
                    catch
                    {
                        continue;
                    }

                    foreach (var tooltip in EnumerateFromObject(value, visitedOwners))
                    {
                        yield return tooltip;
                    }
                }

                type = type.BaseType;
            }
        }
    }
}
