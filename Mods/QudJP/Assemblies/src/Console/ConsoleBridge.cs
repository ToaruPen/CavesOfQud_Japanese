using System;
using System.Text;
using System.Threading;
using ConsoleLib.Console;
using UnityEngine;

namespace QudJP.ConsoleUI
{
    internal sealed class ConsoleBridge
    {
        public const float BaseCellWidth = 8f;
        public const float BaseCellHeight = 16f;
        private const string MonoSpaceTagValue = "0.61";

        public static ConsoleBridge Instance { get; } = new();

        private readonly object _captureGate = new();
        private readonly StringBuilder _lineBuilder = new(160);
        private static readonly char[] Cp437Extended =
        {
            '\u00C7', '\u00FC', '\u00E9', '\u00E2', '\u00E4', '\u00E0', '\u00E5', '\u00E7',
            '\u00EA', '\u00EB', '\u00E8', '\u00EF', '\u00EE', '\u00EC', '\u00C4', '\u00C5',
            '\u00C9', '\u00E6', '\u00C6', '\u00F4', '\u00F6', '\u00F2', '\u00FB', '\u00F9',
            '\u00FF', '\u00D6', '\u00DC', '\u00A2', '\u00A3', '\u00A5', '\u20A7', '\u0192',
            '\u00E1', '\u00ED', '\u00F3', '\u00FA', '\u00F1', '\u00D1', '\u00AA', '\u00BA',
            '\u00BF', '\u2310', '\u00AC', '\u00BD', '\u00BC', '\u00A1', '\u00AB', '\u00BB',
            '\u2591', '\u2592', '\u2593', '\u2502', '\u2524', '\u2561', '\u2562', '\u2556',
            '\u2555', '\u2563', '\u2551', '\u2557', '\u255D', '\u255C', '\u255B', '\u2510',
            '\u2514', '\u2534', '\u252C', '\u251C', '\u2500', '\u253C', '\u255E', '\u255F',
            '\u255A', '\u2554', '\u2569', '\u2566', '\u2560', '\u2550', '\u256C', '\u2567',
            '\u2568', '\u2564', '\u2565', '\u2559', '\u2558', '\u2552', '\u2553', '\u256B',
            '\u256A', '\u2518', '\u250C', '\u2588', '\u2584', '\u258C', '\u2590', '\u2580',
            '\u03B1', '\u00DF', '\u0393', '\u03C0', '\u03A3', '\u03C3', '\u00B5', '\u03C4',
            '\u03A6', '\u0398', '\u03A9', '\u03B4', '\u221E', '\u03C6', '\u03B5', '\u2229',
            '\u2261', '\u00B1', '\u2265', '\u2264', '\u2320', '\u2321', '\u00F7', '\u2248',
            '\u00B0', '\u2219', '\u00B7', '\u221A', '\u207F', '\u00B2', '\u25A0', '\u00A0'
        };

        private ConsoleFrame? _pending;
        private ConsoleBridgeView? _view;
        private bool _viewInitialized;
        private bool _initialized;
        private bool _displayRequested;
        private long _frameId;

        private ConsoleBridge()
        {
        }

        public void Initialize()
        {
            if (_initialized)
            {
                return;
            }

            _initialized = true;
            EnsureView();
        }

        internal bool ShouldDisplay => Volatile.Read(ref _displayRequested);

        internal ConsoleFrame CaptureFrame(ScreenBuffer buffer)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            lock (_captureGate)
            {
                var width = buffer.Width;
                var height = buffer.Height;
                var lines = new string[height];
                var hasContent = false;

                for (var row = 0; row < height; row++)
                {
                    lines[row] = BuildLine(buffer, row, out var lineContent);
                    if (lineContent)
                    {
                        hasContent = true;
                    }
                }

                var id = Interlocked.Increment(ref _frameId);
                return new ConsoleFrame(lines, width, height, BaseCellWidth, BaseCellHeight, id, hasContent);
            }
        }

        internal void PublishFrame(ConsoleFrame frame)
        {
            if (frame == null)
            {
                return;
            }

            if (!_viewInitialized)
            {
                return;
            }

            Volatile.Write(ref _displayRequested, frame.HasContent);
            Interlocked.Exchange(ref _pending, frame);
        }

        internal ConsoleFrame? ConsumeFrame()
        {
            return Interlocked.Exchange(ref _pending, null);
        }

        private void EnsureView()
        {
            if (_viewInitialized)
            {
                return;
            }

#pragma warning disable CS0618
            var existing = UnityEngine.Object.FindObjectOfType<ConsoleBridgeView>();
#pragma warning restore CS0618
            if (existing != null)
            {
                existing.Attach(this);
                _view = existing;
                _viewInitialized = true;
                return;
            }

            var host = new GameObject("QudJP.ConsoleBridge");
            host.hideFlags = HideFlags.HideAndDontSave;
            UnityEngine.Object.DontDestroyOnLoad(host);

            _view = host.AddComponent<ConsoleBridgeView>();
            _view.Attach(this);
            _viewInitialized = true;
        }

        private string BuildLine(ScreenBuffer buffer, int row, out bool hasContent)
        {
            var builder = _lineBuilder;
            builder.Clear();
            builder.Append("<mspace=");
            builder.Append(MonoSpaceTagValue);
            builder.Append('>');

            var width = buffer.Width;
            var lastColor = -1;
            hasContent = false;

            for (var x = 0; x < width; x++)
            {
                var cell = buffer[x, row];
                if (!hasContent && HasRenderableContent(cell))
                {
                    hasContent = true;
                }
                lastColor = AppendColorIfNeeded(builder, cell, lastColor);
                AppendGlyph(builder, cell);
            }

            if (lastColor != -1)
            {
                builder.Append("</color>");
            }

            builder.Append("</mspace>");
            var text = builder.ToString();
            builder.Clear();
            return text;
        }

        private int AppendColorIfNeeded(StringBuilder builder, ConsoleChar cell, int current)
        {
            var colorKey = PackColor(cell.Foreground);
            if (colorKey == current)
            {
                return current;
            }

            if (current != -1)
            {
                builder.Append("</color>");
            }

            builder.Append("<color=#");
            AppendHex(builder, (colorKey >> 16) & 0xFF);
            AppendHex(builder, (colorKey >> 8) & 0xFF);
            AppendHex(builder, colorKey & 0xFF);
            builder.Append('>');

            return colorKey;
        }

        private void AppendGlyph(StringBuilder builder, ConsoleChar cell)
        {
            var tile = cell.Tile;
            if (!string.IsNullOrEmpty(tile))
            {
                AppendEscaped(builder, tile);
                return;
            }

            var glyph = cell.Char;
            if (glyph == '\0' || glyph == '\r' || glyph == '\n' || glyph < 32)
            {
                builder.Append(' ');
                return;
            }

            if (glyph < 128)
            {
                AppendEscaped(builder, glyph);
                return;
            }

            if (glyph <= 255)
            {
                AppendEscaped(builder, DecodeCp437(glyph));
                return;
            }

            AppendEscaped(builder, glyph);
        }

        private static char DecodeCp437(char glyph)
        {
            if (glyph < 128)
            {
                return glyph;
            }

            var index = glyph - 128;
            if ((uint)index < Cp437Extended.Length)
            {
                return Cp437Extended[index];
            }

            return glyph;
        }

        private static bool HasRenderableContent(ConsoleChar cell)
        {
            if (!string.IsNullOrEmpty(cell.Tile))
            {
                return true;
            }

            var glyph = cell.Char;
            if (glyph > 32)
            {
                return true;
            }

            if (glyph >= 128 && glyph <= 255)
            {
                var decoded = DecodeCp437(glyph);
                return decoded > 32;
            }

            return false;
        }

        private static void AppendEscaped(StringBuilder builder, string value)
        {
            for (var i = 0; i < value.Length; i++)
            {
                AppendEscaped(builder, value[i]);
            }
        }

        private static void AppendEscaped(StringBuilder builder, char value)
        {
            switch (value)
            {
                case '<':
                    builder.Append("&lt;");
                    break;
                case '>':
                    builder.Append("&gt;");
                    break;
                case '&':
                    builder.Append("&amp;");
                    break;
                default:
                    if (char.IsControl(value))
                    {
                        builder.Append(' ');
                    }
                    else
                    {
                        builder.Append(value);
                    }
                    break;
            }
        }

        private static int PackColor(Color color)
        {
            var c = (Color32)color;
            return (c.r << 16) | (c.g << 8) | c.b;
        }

        private static void AppendHex(StringBuilder builder, int value)
        {
            builder.Append(GetHexDigit(value >> 4));
            builder.Append(GetHexDigit(value & 0xF));
        }

        private static char GetHexDigit(int value)
        {
            return (char)(value < 10 ? '0' + value : 'A' + (value - 10));
        }
    }

    internal sealed class ConsoleFrame
    {
        public ConsoleFrame(
            string[] lines,
            int width,
            int height,
            float cellWidth,
            float cellHeight,
            long frameId,
            bool hasContent)
        {
            Lines = lines;
            Width = width;
            Height = height;
            BaseCellWidth = cellWidth;
            BaseCellHeight = cellHeight;
            FrameId = frameId;
            HasContent = hasContent;
        }

        public string[] Lines { get; }

        public int Width { get; }

        public int Height { get; }

        public float BaseCellWidth { get; }

        public float BaseCellHeight { get; }

        public long FrameId { get; }

        public bool HasContent { get; }
    }
}
