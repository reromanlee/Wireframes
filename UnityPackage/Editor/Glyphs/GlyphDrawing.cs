using UnityEngine;
using UnityEngine.UIElements;

namespace reromanlee.Wireframes.Editor
{
    /// <summary>Draws glyphs with UI Toolkit's vector API, mapping the glyph box, 0 to 1 with +Y up, onto a rectangle.</summary>
    internal static class GlyphDrawing
    {
        /// <summary>Where a glyph-box point lands in <paramref name="box"/>, whose top is the box's 1.</summary>
        internal static Vector2 ToBox(Vector2 point, Rect box)
        {
            return new Vector2(box.x + point.x * box.width, box.yMax - point.y * box.height);
        }

        /// <summary>The glyph-box point at <paramref name="position"/> in <paramref name="box"/>.</summary>
        internal static Vector2 FromBox(Vector2 position, Rect box)
        {
            return new Vector2((position.x - box.x) / box.width, (box.yMax - position.y) / box.height);
        }

        /// <summary>Strokes the lines of every stroke with at least 2 points, closed strokes back to their start.</summary>
        internal static void DrawStrokes(Painter2D painter, GlyphStroke[] strokes, Rect box, Color color, float width)
        {
            painter.strokeColor = color;
            painter.lineWidth = width;
            painter.lineJoin = LineJoin.Round;
            painter.lineCap = LineCap.Round;
            foreach (GlyphStroke stroke in strokes)
            {
                DrawStroke(painter, stroke, box);
            }
        }

        internal static void DrawStroke(Painter2D painter, GlyphStroke stroke, Rect box)
        {
            Vector2[] points = stroke.Points;
            if (points.Length < 2)
            {
                return;
            }
            painter.BeginPath();
            painter.MoveTo(ToBox(points[0], box));
            for (int i = 1; i < points.Length; i++)
            {
                painter.LineTo(ToBox(points[i], box));
            }
            if (stroke.IsClosed)
            {
                painter.ClosePath();
            }
            painter.Stroke();
        }

        /// <summary>A straight line from <paramref name="from"/> to <paramref name="to"/>, in element coordinates.</summary>
        internal static void DrawLine(Painter2D painter, Vector2 from, Vector2 to, Color color, float width)
        {
            painter.strokeColor = color;
            painter.lineWidth = width;
            painter.BeginPath();
            painter.MoveTo(from);
            painter.LineTo(to);
            painter.Stroke();
        }

        /// <summary>The outline of <paramref name="rect"/>.</summary>
        internal static void DrawRect(Painter2D painter, Rect rect, Color color, float width)
        {
            painter.strokeColor = color;
            painter.lineWidth = width;
            painter.BeginPath();
            painter.MoveTo(new Vector2(rect.xMin, rect.yMin));
            painter.LineTo(new Vector2(rect.xMax, rect.yMin));
            painter.LineTo(new Vector2(rect.xMax, rect.yMax));
            painter.LineTo(new Vector2(rect.xMin, rect.yMax));
            painter.ClosePath();
            painter.Stroke();
        }

        /// <summary>The largest square that fits <paramref name="rect"/>, centered in it, shrunk by <paramref name="margin"/>.</summary>
        internal static Rect FittedBox(Rect rect, float margin)
        {
            float size = Mathf.Max(0f, Mathf.Min(rect.width, rect.height) - margin * 2f);
            return new Rect(rect.center.x - size * 0.5f, rect.center.y - size * 0.5f, size, size);
        }

        /// <summary>A color with <paramref name="alpha"/> times its own alpha.</summary>
        internal static Color Faded(Color color, float alpha)
        {
            return new Color(color.r, color.g, color.b, color.a * alpha);
        }
    }
}
