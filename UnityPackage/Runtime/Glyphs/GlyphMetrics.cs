using System;
using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// What a glyph's drawn strokes take up: the extent of their ink, which proportional text advances by, and the
    /// vertices and edges they need, so a layout can size its buffers before writing anything.
    /// </summary>
    internal readonly struct GlyphMetrics
    {
        internal readonly float InkLeft;
        internal readonly float InkRight;
        internal readonly float InkBottom;
        internal readonly float InkTop;
        internal readonly int VertexCount;
        internal readonly int EdgeCount;

        private GlyphMetrics(float inkLeft, float inkRight, float inkBottom, float inkTop, int vertexCount, int edgeCount)
        {
            InkLeft = inkLeft;
            InkRight = inkRight;
            InkBottom = inkBottom;
            InkTop = inkTop;
            VertexCount = vertexCount;
            EdgeCount = edgeCount;
        }

        /// <summary>True when at least one stroke draws, so the glyph has ink to measure.</summary>
        internal bool HasInk
        {
            get => EdgeCount > 0;
        }

        /// <summary>Width of the ink, from its leftmost to its rightmost point; zero without ink.</summary>
        internal float InkWidth
        {
            get => InkRight - InkLeft;
        }

        internal static GlyphMetrics Measure(GlyphStroke[] strokes)
        {
            float left = float.PositiveInfinity;
            float right = float.NegativeInfinity;
            float bottom = float.PositiveInfinity;
            float top = float.NegativeInfinity;
            int vertexCount = 0;
            int edgeCount = 0;
            foreach (GlyphStroke stroke in strokes)
            {
                if (!stroke.IsDrawn)
                {
                    continue;
                }
                foreach (Vector2 point in stroke.Points)
                {
                    left = Math.Min(left, point.x);
                    right = Math.Max(right, point.x);
                    bottom = Math.Min(bottom, point.y);
                    top = Math.Max(top, point.y);
                }
                vertexCount += stroke.Points.Length;
                edgeCount += stroke.EdgeCount;
            }
            return edgeCount > 0 ? new GlyphMetrics(left, right, bottom, top, vertexCount, edgeCount) : default;
        }
    }
}
