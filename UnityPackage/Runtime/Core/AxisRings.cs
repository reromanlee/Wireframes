using System;
using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// Three rings around a center, one in each of the XZ, XY and YZ planes: the way spheres and ellipsoids are drawn.
    /// </summary>
    internal static class AxisRings
    {
        internal const int RingCount = 3;

        internal static readonly PatternCache Patterns = new(BuildEdges);

        /// <summary>Writes the three rings, each with a third of the positions, with the given radius along each axis.</summary>
        internal static void Write(Span<Vector3> positions, Vector3 radii)
        {
            int segments = positions.Length / RingCount;
            Vector3 x = new(radii.x, 0f, 0f);
            Vector3 y = new(0f, radii.y, 0f);
            Vector3 z = new(0f, 0f, radii.z);
            Ring.Write(positions.Slice(0, segments), Vector3.zero, z, x);
            Ring.Write(positions.Slice(segments, segments), Vector3.zero, y, x);
            Ring.Write(positions.Slice(segments * 2, segments), Vector3.zero, y, z);
        }

        private static int[] BuildEdges(int segments)
        {
            int[] pattern = new int[segments * RingCount * 2];
            int cursor = 0;
            for (int ring = 0; ring < RingCount; ring++)
            {
                Ring.AddEdges(pattern, ref cursor, ring * segments, segments);
            }
            return pattern;
        }
    }
}
