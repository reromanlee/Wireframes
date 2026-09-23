using System;
using System.Collections.Generic;
using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// A Platonic solid that a spiked sphere is built on, with unit-length corners and the direction of each face's
    /// center, in the solid's usual orientation along the axes. Built once per face count and shared.
    /// </summary>
    internal sealed class PlatonicSolid
    {
        // Precise enough to group corners of the same face and edges of the same length, for unit-sized solids.
        private const float Tolerance = 1e-4f;
        private static readonly float GoldenRatio = (1f + Mathf.Sqrt(5f)) * 0.5f;
        private static readonly Dictionary<int, PlatonicSolid> Solids = new();

        private PlatonicSolid(Vector3[] corners, Vector3[] faces)
        {
            Corners = corners;
            Faces = faces;
            EdgePattern = BuildEdges(corners, faces);
        }

        internal Vector3[] Corners { get; }

        /// <summary>Directions from the center through the middle of each face, where the spikes point.</summary>
        internal Vector3[] Faces { get; }

        /// <summary>
        /// The solid's edges, then an edge from each spike tip to every corner of its face. Vertices are the corners
        /// followed by one tip per face.
        /// </summary>
        internal int[] EdgePattern { get; }

        internal static bool HasFaceCount(int faceCount)
        {
            return faceCount is 4 or 6 or 8 or 12 or 20;
        }

        /// <summary>Returns the solid with <paramref name="faceCount"/> faces; <see cref="HasFaceCount"/> must accept it.</summary>
        internal static PlatonicSolid WithFaces(int faceCount)
        {
            if (!Solids.TryGetValue(faceCount, out PlatonicSolid solid))
            {
                solid = faceCount switch
                {
                    4 => Tetrahedron(),
                    6 => new PlatonicSolid(CubeCorners(), OctahedronCorners()),
                    8 => new PlatonicSolid(OctahedronCorners(), CubeCorners()),
                    12 => new PlatonicSolid(FaceCenters(IcosahedronCorners()), IcosahedronCorners()),
                    20 => new PlatonicSolid(IcosahedronCorners(), FaceCenters(IcosahedronCorners())),
                    _ => throw new ArgumentOutOfRangeException(nameof(faceCount))
                };
                Solids.Add(faceCount, solid);
            }
            return solid;
        }

        private static PlatonicSolid Tetrahedron()
        {
            // Every other corner of a cube; each face lies opposite a corner.
            Vector3[] corners = Normalized(
                new Vector3(1f, 1f, 1f), new Vector3(1f, -1f, -1f), new Vector3(-1f, 1f, -1f), new Vector3(-1f, -1f, 1f));
            Vector3[] faces = new Vector3[corners.Length];
            for (int i = 0; i < corners.Length; i++)
            {
                faces[i] = -corners[i];
            }
            return new PlatonicSolid(corners, faces);
        }

        private static Vector3[] CubeCorners()
        {
            List<Vector3> corners = new();
            for (int i = 0; i < 8; i++)
            {
                corners.Add(new Vector3((i & 1) == 0 ? -1f : 1f, (i & 2) == 0 ? -1f : 1f, (i & 4) == 0 ? -1f : 1f));
            }
            return Normalized(corners.ToArray());
        }

        private static Vector3[] OctahedronCorners()
        {
            return new[] { Vector3.right, Vector3.left, Vector3.up, Vector3.down, Vector3.forward, Vector3.back };
        }

        private static Vector3[] IcosahedronCorners()
        {
            // The three golden rectangles (0, ±1, ±φ) in cyclic order.
            List<Vector3> corners = new();
            foreach (float one in new[] { -1f, 1f })
            {
                foreach (float phi in new[] { -GoldenRatio, GoldenRatio })
                {
                    corners.Add(new Vector3(0f, one, phi));
                    corners.Add(new Vector3(one, phi, 0f));
                    corners.Add(new Vector3(phi, 0f, one));
                }
            }
            return Normalized(corners.ToArray());
        }

        /// <summary>
        /// Directions through the middle of each triangular face of a solid: every three corners that are all one edge
        /// apart. The faces of a solid are the corners of its dual, so this also gives the dodecahedron from the
        /// icosahedron.
        /// </summary>
        private static Vector3[] FaceCenters(Vector3[] corners)
        {
            float edge = ShortestDistance(corners);
            List<Vector3> centers = new();
            for (int a = 0; a < corners.Length; a++)
            {
                for (int b = a + 1; b < corners.Length; b++)
                {
                    for (int c = b + 1; c < corners.Length; c++)
                    {
                        if (IsEdge(corners[a], corners[b], edge) && IsEdge(corners[b], corners[c], edge) &&
                            IsEdge(corners[a], corners[c], edge))
                        {
                            centers.Add((corners[a] + corners[b] + corners[c]).normalized);
                        }
                    }
                }
            }
            return centers.ToArray();
        }

        private static int[] BuildEdges(Vector3[] corners, Vector3[] faces)
        {
            List<int> pattern = new();
            // All edges of a Platonic solid have the same, shortest, length.
            float edge = ShortestDistance(corners);
            for (int a = 0; a < corners.Length; a++)
            {
                for (int b = a + 1; b < corners.Length; b++)
                {
                    if (IsEdge(corners[a], corners[b], edge))
                    {
                        pattern.Add(a);
                        pattern.Add(b);
                    }
                }
            }
            // A face's corners are the ones closest to its middle.
            for (int face = 0; face < faces.Length; face++)
            {
                float closest = float.MinValue;
                foreach (Vector3 corner in corners)
                {
                    closest = Mathf.Max(closest, Vector3.Dot(corner, faces[face]));
                }
                for (int corner = 0; corner < corners.Length; corner++)
                {
                    if (Vector3.Dot(corners[corner], faces[face]) > closest - Tolerance)
                    {
                        pattern.Add(corners.Length + face);
                        pattern.Add(corner);
                    }
                }
            }
            return pattern.ToArray();
        }

        private static float ShortestDistance(Vector3[] points)
        {
            float shortest = float.MaxValue;
            for (int a = 0; a < points.Length; a++)
            {
                for (int b = a + 1; b < points.Length; b++)
                {
                    shortest = Mathf.Min(shortest, Vector3.Distance(points[a], points[b]));
                }
            }
            return shortest;
        }

        private static bool IsEdge(Vector3 a, Vector3 b, float edge)
        {
            return Vector3.Distance(a, b) < edge + Tolerance;
        }

        private static Vector3[] Normalized(params Vector3[] points)
        {
            for (int i = 0; i < points.Length; i++)
            {
                points[i] = points[i].normalized;
            }
            return points;
        }
    }
}
