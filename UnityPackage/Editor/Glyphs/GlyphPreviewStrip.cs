using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

namespace reromanlee.Wireframes.Editor
{
    /// <summary>
    /// Sample text drawn with the pack alone, laid out as texts lay it out, proportional or monospace. A character the
    /// pack lacks is drawn as its '?' in a warning color and listed below, instead of warning in the Console.
    /// </summary>
    internal sealed class GlyphPreviewStrip : VisualElement
    {
        private const float Padding = 8f;

        private readonly TextField _text = new() { multiline = true };
        private readonly Toggle _monospace = new("Monospace");
        private readonly Label _missing = new();
        private readonly VisualElement _preview = new();
        private readonly GlyphGeometry _geometry = new();
        private readonly List<RangeInt> _fallbacks = new();
        private readonly StringBuilder _missingText = new();
        private readonly HashSet<int> _missingCharacters = new();

        private WireframeGlyphs _glyphs;
        private int _lineCount = 1;

        internal GlyphPreviewStrip(string text, bool isMonospace)
        {
            AddToClassList("glyph-preview-strip");
            VisualElement controls = new();
            controls.AddToClassList("glyph-preview-strip__controls");
            _text.AddToClassList("glyph-preview-strip__text");
            _text.SetValueWithoutNotify(text);
            _text.tooltip = "Text to preview with this pack.";
            _text.RegisterValueChangedCallback(change =>
            {
                TextChanged?.Invoke(change.newValue);
                Refresh();
            });
            _monospace.SetValueWithoutNotify(isMonospace);
            _monospace.tooltip = "Give every character a whole glyph box, as monospace text does.";
            _monospace.RegisterValueChangedCallback(change =>
            {
                MonospaceChanged?.Invoke(change.newValue);
                Refresh();
            });
            _missing.AddToClassList("glyph-preview-strip__missing");
            controls.Add(_text);
            controls.Add(_monospace);
            _preview.AddToClassList("glyph-preview-strip__preview");
            _preview.generateVisualContent += Draw;
            Add(controls);
            Add(_preview);
            Add(_missing);
        }

        /// <summary>Raised when the sample text changes, so the window keeps it for next time.</summary>
        internal event Action<string> TextChanged;

        /// <summary>Raised when monospace is switched, so the window keeps it for next time.</summary>
        internal event Action<bool> MonospaceChanged;

        /// <summary>Previews with <paramref name="glyphs"/>, a glyph list that holds only the open pack.</summary>
        internal void Show(WireframeGlyphs glyphs)
        {
            _glyphs = glyphs;
            Refresh();
        }

        /// <summary>Lays the sample text out again, after it changed or the pack did.</summary>
        internal void Refresh()
        {
            if (_glyphs == null)
            {
                return;
            }
            string text = _text.value ?? string.Empty;
            TextSettings settings = TextSettings.Default;
            settings.CharacterWidth = _monospace.value ? WireframeCharacterWidth.Monospace : WireframeCharacterWidth.Proportional;
            settings.Bounds = Vector2.zero;
            settings.HorizontalAlignment = WireframeHorizontalAlignment.Left;
            settings.VerticalAlignment = WireframeVerticalAlignment.Top;
            TextLayout.Layout(text, _glyphs, settings, _geometry, null, _fallbacks);
            _lineCount = 1;
            foreach (char character in text)
            {
                if (character == '\n')
                {
                    _lineCount++;
                }
            }
            ShowMissing(text);
            _preview.MarkDirtyRepaint();
        }

        private void ShowMissing(string text)
        {
            _missingCharacters.Clear();
            _missingText.Clear();
            for (int i = 0; i < text.Length; i++)
            {
                int character = text[i];
                if (char.IsHighSurrogate(text[i]) && i + 1 < text.Length && char.IsLowSurrogate(text[i + 1]))
                {
                    character = char.ConvertToUtf32(text[i], text[++i]);
                }
                if (character == ' ' || character < 0x10000 && char.IsControl((char)character)
                    || _glyphs.TryResolveCharacter(character, out _) || !_missingCharacters.Add(character))
                {
                    continue;
                }
                _missingText.Append(_missingText.Length == 0 ? "Missing from this pack: " : " ");
                _missingText.Append(GlyphNames.CharacterOf(character));
            }
            _missing.text = _missingText.ToString();
            _missing.style.display = _missingText.Length > 0 ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void Draw(MeshGenerationContext context)
        {
            int edgeCount = _geometry.EdgeCount;
            Rect area = _preview.contentRect;
            if (edgeCount == 0 || area.width <= Padding * 2f || area.height <= Padding * 2f)
            {
                return;
            }
            Vector3[] points = _geometry.Points;
            float width = 0f;
            for (int i = 0; i < _geometry.VertexCount; i++)
            {
                width = Mathf.Max(width, points[i].x);
            }
            float scale = Mathf.Min((area.height - Padding * 2f) / _lineCount, (area.width - Padding * 2f) / Mathf.Max(width, 1f));
            Painter2D painter = context.painter2D;
            painter.lineWidth = Mathf.Clamp(scale * 0.04f, 1f, 2f);
            painter.lineCap = LineCap.Round;
            painter.lineJoin = LineJoin.Round;
            DrawEdges(painter, scale, false, _preview.resolvedStyle.color);
            DrawEdges(painter, scale, true, new Color(1f, 0.65f, 0.1f, 1f));
        }

        /// <summary>Strokes the edges of the '?' stand-ins, or every other edge, as one path.</summary>
        private void DrawEdges(Painter2D painter, float scale, bool areFallbacks, Color color)
        {
            Vector3[] points = _geometry.Points;
            int[] pattern = _geometry.Pattern;
            painter.strokeColor = color;
            painter.BeginPath();
            for (int edge = 0; edge < _geometry.EdgeCount; edge++)
            {
                int a = pattern[edge * 2];
                if (IsFallback(a) != areFallbacks)
                {
                    continue;
                }
                painter.MoveTo(ToPreview(points[a], scale));
                painter.LineTo(ToPreview(points[pattern[edge * 2 + 1]], scale));
            }
            painter.Stroke();
        }

        private bool IsFallback(int vertex)
        {
            foreach (RangeInt range in _fallbacks)
            {
                if (vertex >= range.start && vertex < range.end)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>Text is laid out from the top left corner down, which the preview's top left corner takes.</summary>
        private static Vector2 ToPreview(Vector3 point, float scale)
        {
            return new Vector2(Padding + point.x * scale, Padding - point.y * scale);
        }
    }
}
