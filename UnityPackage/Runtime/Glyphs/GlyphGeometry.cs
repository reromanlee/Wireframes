using System;
using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// The points and edges of laid-out glyphs, kept in buffers with room to spare: they grow by doubling and shrink only
    /// once a quarter is used, so a text that keeps changing reuses them instead of allocating. Their capacity is the
    /// vertex count the shape takes in its chunk, and points past <see cref="VertexCount"/> are never joined by an edge.
    /// </summary>
    internal sealed class GlyphGeometry
    {
        private const int MinimumCapacity = 16;

        private Vector3[] _points = new Vector3[MinimumCapacity];
        private int[] _pattern = new int[MinimumCapacity * 2];

        /// <summary>Points written so far, in the shape's local space.</summary>
        internal int VertexCount { get; private set; }

        /// <summary>Edges written so far, as pairs in <see cref="Pattern"/>.</summary>
        internal int EdgeCount { get; private set; }

        /// <summary>Points the buffers hold, at least 16.</summary>
        internal int VertexCapacity
        {
            get => _points.Length;
        }

        internal Vector3[] Points
        {
            get => _points;
        }

        /// <summary>Pairs of indices into <see cref="Points"/>, one pair per edge.</summary>
        internal int[] Pattern
        {
            get => _pattern;
        }

        /// <summary>Empties the buffers and makes room for <paramref name="vertexCount"/> points and <paramref name="edgeCount"/> edges.</summary>
        internal void Begin(int vertexCount, int edgeCount)
        {
            VertexCount = 0;
            EdgeCount = 0;
            int vertexCapacity = FittedCapacity(_points.Length, vertexCount);
            if (vertexCapacity != _points.Length)
            {
                _points = new Vector3[vertexCapacity];
            }
            int edgeCapacity = FittedCapacity(_pattern.Length / 2, edgeCount);
            if (edgeCapacity != _pattern.Length / 2)
            {
                _pattern = new int[edgeCapacity * 2];
            }
        }

        /// <summary>
        /// Writes the strokes that draw, as <see cref="GlyphStroke.IsDrawn"/> tells, with each glyph-box point placed at
        /// <paramref name="origin"/> plus the point scaled by <paramref name="scale"/> in the XY plane.
        /// </summary>
        internal void AddGlyph(GlyphStroke[] strokes, Vector2 origin, float scale)
        {
            foreach (GlyphStroke stroke in strokes)
            {
                if (!stroke.IsDrawn)
                {
                    continue;
                }
                Vector2[] points = stroke.Points;
                int first = VertexCount;
                for (int i = 0; i < points.Length; i++)
                {
                    _points[first + i] = new Vector3(origin.x + points[i].x * scale, origin.y + points[i].y * scale, 0f);
                }
                VertexCount += points.Length;
                int last = VertexCount - 1;
                for (int vertex = first; vertex < last; vertex++)
                {
                    AddEdge(vertex, vertex + 1);
                }
                if (stroke.IsClosed)
                {
                    AddEdge(last, first);
                }
            }
        }

        private void AddEdge(int vertexA, int vertexB)
        {
            _pattern[EdgeCount * 2] = vertexA;
            _pattern[EdgeCount * 2 + 1] = vertexB;
            EdgeCount++;
        }

        /// <summary>
        /// The capacity to hold <paramref name="required"/>: doubled from the current one when it is too small, halved
        /// while a quarter or less of it is used, and never below <see cref="MinimumCapacity"/>.
        /// </summary>
        private static int FittedCapacity(int capacity, int required)
        {
            while (capacity < required)
            {
                capacity *= 2;
            }
            while (capacity > MinimumCapacity && required <= capacity / 4)
            {
                capacity /= 2;
            }
            return capacity;
        }
    }
}
