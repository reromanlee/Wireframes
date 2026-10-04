using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace reromanlee.Wireframes.Editor
{
    /// <summary>
    /// A glyph drawn in its box, with the box and the baseline faint behind it, in the element's text color, for the
    /// gallery's tiles.
    /// </summary>
    internal sealed class GlyphPreview : VisualElement
    {
        private const float Margin = 4f;

        private GlyphStroke[] _strokes = Array.Empty<GlyphStroke>();

        internal GlyphPreview()
        {
            AddToClassList("glyph-preview");
            pickingMode = PickingMode.Ignore;
            generateVisualContent += Draw;
        }

        internal GlyphStroke[] Strokes
        {
            get => _strokes;
            set
            {
                _strokes = value ?? Array.Empty<GlyphStroke>();
                MarkDirtyRepaint();
            }
        }

        private void Draw(MeshGenerationContext context)
        {
            Rect box = GlyphDrawing.FittedBox(contentRect, Margin);
            if (box.width <= 0f)
            {
                return;
            }
            Painter2D painter = context.painter2D;
            Color color = resolvedStyle.color;
            GlyphDrawing.DrawRect(painter, box, GlyphDrawing.Faded(color, 0.15f), 1f);
            float baseline = GlyphDrawing.ToBox(new Vector2(0f, WireframeGlyphPack.Baseline), box).y;
            GlyphDrawing.DrawLine(painter, new Vector2(box.xMin, baseline), new Vector2(box.xMax, baseline),
                GlyphDrawing.Faded(color, 0.15f), 1f);
            GlyphDrawing.DrawStrokes(painter, _strokes, box, color, 1.5f);
        }
    }
}
