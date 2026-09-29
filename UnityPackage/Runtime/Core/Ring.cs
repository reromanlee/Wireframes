using System;
using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>Points and edges of the closed rings that round shapes are made of.</summary>
    internal static class Ring
    {
        internal const int DefaultSegmentCount = 32;

        /// <summary>Most segments a ring can have, which keeps a shape and its shared edge pattern to a sane size.</summary>
        internal const int MaxSegmentCount = 1024;

        /// <summary>Edges of shapes that are one closed ring, by vertex count.</summary>
        internal static readonly PatternCache Patterns = new(BuildEdges);

        /// <summary>Returns <paramref name="segmentCount"/> once it is checked, so it can be used in a base constructor call.</summary>
        internal static int CheckSegmentCount(int segmentCount)
        {
            if (segmentCount < 3 || segmentCount > MaxSegmentCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(segmentCount), segmentCount, $"A ring needs 3 to {MaxSegmentCount} segments.");
            }
            return segmentCount;
        }

        /// <summary>
        /// Returns <paramref name="segmentCount"/> once it is checked to be a multiple of 4, which shapes need when their
        /// lines meet a ring at its quarter points.
        /// </summary>
        internal static int CheckQuarterSegmentCount(int segmentCount)
        {
            if (segmentCount < 4 || segmentCount % 4 != 0 || segmentCount > MaxSegmentCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(segmentCount),
                    segmentCount,
                    $"This shape needs a multiple of 4 segments, from 4 to {MaxSegmentCount}.");
            }
            return segmentCount;
        }

        /// <summary>
        /// Writes a closed ring with one point per element of <paramref name="positions"/>. Point i sits at
        /// <paramref name="center"/> + cos(a) * <paramref name="axisU"/> + sin(a) * <paramref name="axisV"/>, where
        /// a is i / count of a full turn, so the ring starts on U and passes V a quarter turn later.
        /// </summary>
        internal static void Write(Span<Vector3> positions, Vector3 center, Vector3 axisU, Vector3 axisV)
        {
            CirclePoints circle = new(positions.Length);
            for (int i = 0; i < positions.Length; i++)
            {
                Vector2 point = circle.Next();
                positions[i] = center + axisU * point.x + axisV * point.y;
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

        private static int[] BuildEdges(int count)
        {
            int[] pattern = new int[count * 2];
            int cursor = 0;
            AddEdges(pattern, ref cursor, 0, count);
            return pattern;
        }
    }
}
