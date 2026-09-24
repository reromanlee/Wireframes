using System;
using System.Collections.Generic;
using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>Points and edges of the closed rings that round shapes are made of.</summary>
    internal static class Ring
    {
        internal const int DefaultSegments = 32;

        /// <summary>Edges of shapes that are one closed ring, by vertex count.</summary>
        internal static readonly PatternCache Patterns = new(BuildEdges);

        // Cosine and sine around a unit circle for each segment count in use, shared by every ring with that count.
        private static readonly Dictionary<int, Vector2[]> UnitCircles = new();

        /// <summary>Returns <paramref name="segments"/> once it is checked, so it can be used in a base constructor call.</summary>
        internal static int CheckSegments(int segments)
        {
            if (segments < 3)
            {
                throw new ArgumentOutOfRangeException(nameof(segments), segments, "A ring needs at least 3 segments.");
            }
            return segments;
        }

        /// <summary>
        /// Returns <paramref name="segments"/> once it is checked to be a multiple of 4, which shapes need when their
        /// lines meet a ring at its quarter points.
        /// </summary>
        internal static int CheckQuarterSegments(int segments)
        {
            if (segments < 4 || segments % 4 != 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(segments), segments, "This shape needs a positive multiple of 4 segments.");
            }
            return segments;
        }

        /// <summary>
        /// Writes a closed ring with one point per element of <paramref name="positions"/>. Point i sits at
        /// <paramref name="center"/> + cos(a) * <paramref name="axisU"/> + sin(a) * <paramref name="axisV"/>, where
        /// a is i / count of a full turn, so the ring starts on U and passes V a quarter turn later.
        /// </summary>
        internal static void Write(Span<Vector3> positions, Vector3 center, Vector3 axisU, Vector3 axisV)
        {
            Vector2[] circle = UnitCircle(positions.Length);
            for (int i = 0; i < positions.Length; i++)
            {
                positions[i] = center + axisU * circle[i].x + axisV * circle[i].y;
            }
        }

        /// <summary>
        /// Adds the edges of a closed ring of <paramref name="count"/> vertices, starting at vertex <paramref name="first"/>.
        /// </summary>
        internal static void AddEdges(int[] pattern, ref int cursor, int first, int count)
        {
            for (int i = 0; i < count; i++)
            {
                pattern[cursor++] = first + i;
                pattern[cursor++] = first + (i + 1) % count;
            }
        }

        /// <summary>
        /// Cosine and sine of <paramref name="count"/> angles evenly spread over a full turn, starting at 0. The table
        /// is shared, so callers must not change it.
        /// </summary>
        internal static Vector2[] UnitCircle(int count)
        {
            if (!UnitCircles.TryGetValue(count, out Vector2[] circle))
            {
                circle = new Vector2[count];
                for (int i = 0; i < count; i++)
                {
                    double angle = 2.0 * Math.PI * i / count;
                    circle[i] = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                }
                UnitCircles.Add(count, circle);
            }
            return circle;
        }

        private static int[] BuildEdges(int count)
        {
            int[] pattern = new int[count * 2];
            int cursor = 0;
            AddEdges(pattern, ref cursor, 0, count);
            return pattern;
        }
    }
}
