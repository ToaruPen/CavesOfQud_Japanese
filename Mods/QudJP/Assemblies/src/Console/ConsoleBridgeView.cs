using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace QudJP.ConsoleUI
{
    [DisallowMultipleComponent]
    internal sealed class ConsoleBridgeView : MonoBehaviour
    {
        private ConsoleBridge? _owner;
        private Canvas? _canvas;
        private RectTransform? _content;
        private readonly List<TMP_Text> _lines = new();
        private readonly List<string> _appliedLines = new();

        internal void Attach(ConsoleBridge owner)
        {
            _owner = owner;
        }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);

            _canvas = gameObject.GetComponent<Canvas>() ?? gameObject.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = short.MaxValue;

            var scaler = gameObject.GetComponent<CanvasScaler>() ?? gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            _content = new GameObject("ConsoleContent", typeof(RectTransform)).GetComponent<RectTransform>();
            _content.SetParent(transform, false);
            _content.anchorMin = new Vector2(0.5f, 0.5f);
            _content.anchorMax = new Vector2(0.5f, 0.5f);
            _content.pivot = new Vector2(0.5f, 0.5f);
        }

        private void Update()
        {
            if (_owner == null || _canvas == null || _content == null)
            {
                return;
            }

            var shouldDisplay = _owner.ShouldDisplay;
            _canvas.enabled = shouldDisplay;
            if (!shouldDisplay)
            {
                return;
            }

            var frame = _owner.ConsumeFrame();
            if (frame == null)
            {
                return;
            }

            ApplyFrame(frame);
        }

        private void ApplyFrame(ConsoleFrame frame)
        {
            var width = frame.Width;
            var height = frame.Height;
            if (width <= 0 || height <= 0)
            {
                return;
            }

            var baseWidth = width * frame.BaseCellWidth;
            var baseHeight = height * frame.BaseCellHeight;
            var scaleX = Screen.width / baseWidth;
            var scaleY = Screen.height / baseHeight;
            var scale = Mathf.Min(scaleX, scaleY);

            var cellWidth = frame.BaseCellWidth * scale;
            var cellHeight = frame.BaseCellHeight * scale;
            var contentWidth = width * cellWidth;
            var contentHeight = height * cellHeight;

            _content!.sizeDelta = new Vector2(contentWidth, contentHeight);
            _content.anchoredPosition = Vector2.zero;

            var leftEdge = -contentWidth * 0.5f;
            var topEdge = contentHeight * 0.5f;

            for (var row = 0; row < height; row++)
            {
                var label = GetLine(row);
                var rect = label.rectTransform;
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(0f, 1f);
                rect.sizeDelta = new Vector2(contentWidth, cellHeight);
                rect.anchoredPosition = new Vector2(leftEdge, topEdge - row * cellHeight);

                var next = frame.Lines.Length > row ? frame.Lines[row] ?? string.Empty : string.Empty;
                if (!string.Equals(_appliedLines[row], next, StringComparison.Ordinal))
                {
                    label.text = next;
                    _appliedLines[row] = next;
                }

                label.fontSize = Mathf.Max(cellHeight * 0.9f, 8f);
                label.enabled = true;
            }

            for (var row = height; row < _lines.Count; row++)
            {
                var label = _lines[row];
                label.enabled = false;
                label.gameObject.SetActive(false);
            }
        }

        private TMP_Text GetLine(int index)
        {
            while (_lines.Count <= index)
            {
                var lineObject = new GameObject($"ConsoleLine{_lines.Count:D2}", typeof(RectTransform));
                lineObject.transform.SetParent(_content, false);

                var text = lineObject.AddComponent<TextMeshProUGUI>();
                text.raycastTarget = false;
                text.richText = true;
                text.textWrappingMode = TextWrappingModes.NoWrap;
                text.alignment = TextAlignmentOptions.TopLeft;
                text.margin = Vector4.zero;
                text.lineSpacing = 0f;
                text.characterSpacing = 0f;
                text.overflowMode = TextOverflowModes.Truncate;
                text.extraPadding = true;
                FontManager.Instance.ApplyToText(text, forceReplace: true);

                _lines.Add(text);
                _appliedLines.Add(string.Empty);
            }

            var line = _lines[index];
            line.gameObject.SetActive(true);
            return line;
        }
    }
}
