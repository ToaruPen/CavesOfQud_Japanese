using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace QudJP.Diagnostics
{
    internal static class UIContext
    {
        private static readonly ConditionalWeakTable<object, EidHolder> Bindings = new();
        [ThreadStatic]
        private static Stack<string>? _stack;

        public static string Capture(string? eid = null)
        {
            eid ??= JpLog.NewEID();
            (_stack ??= new Stack<string>()).Push(eid);
            return eid;
        }

        public static void Release(string? eid = null)
        {
            if (_stack == null || _stack.Count == 0)
            {
                return;
            }

            if (eid == null || _stack.Peek() == eid)
            {
                _stack.Pop();
            }
        }

        public static string Bind(object target, string? eid = null)
        {
            if (target == null)
            {
                return Capture(eid);
            }

            eid ??= JpLog.NewEID();
            Bindings.Remove(target);
            Bindings.Add(target, new EidHolder(eid));
            return eid;
        }

        public static string? Current => (_stack != null && _stack.Count > 0) ? _stack.Peek() : null;

        public static string? Resolve(object? target)
        {
            if (target != null && Bindings.TryGetValue(target, out var holder))
            {
                return holder.Eid;
            }

            return Current;
        }

        private sealed class EidHolder
        {
            public EidHolder(string eid) => Eid = eid;
            public string Eid { get; }
        }
    }
}
