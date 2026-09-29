using System;
using UnityEngine;

namespace reromanlee.Wireframes
{
    internal sealed class Capsule : AxialShape, ICapsule
    {
        private const int SideLineCount = 4;
        private static readonly PatternCache Patterns = new(BuildEdges);

        private float _radiusA;
        private float _radiusB;

        internal Capsule(
            MeshProxy proxy,
            Transform bone,
            Vector3 localPosition,
            Quaternion localRotation,
            float length,
            float radiusA,
            float radiusB,
            int segmentCount)
            : base(
                proxy,
                CountVertices(Ring.CheckQuarterSegmentCount(segmentCount)),
                Patterns.Get(segmentCount),
                bone,
                localPosition,
                localRotation,
                length)
        {
            _radiusA = radiusA;
            _radiusB = radiusB;
        }

        public float RadiusA
        {
            get
            {
                EnsureUsable();
                return _radiusA;
            }
            set
            {
                EnsureUsable();
                _radiusA = value;
                MarkDirty(DirtyFlags.Positions);
            }
        }

        public float RadiusB
        {
            get
            {
                EnsureUsable();
                return _radiusB;
            }
            set
            {
                EnsureUsable();
                _radiusB = value;
                MarkDirty(DirtyFlags.Positions);
            }
        }

        public int SegmentCount
        {
            get
            {
                EnsureUsable();
                return (VertexCount + 6) / 4;
            }
        }

        // Vertices: the ring on sphere A, the ring on sphere B, then the inside points of cap A's arcs and cap B's arcs.
        protected override void WriteShape(Span<Vector3> positions, float length)
        {
            int segmentCount = (positions.Length + 6) / 4;
            int capVertices = segmentCount - 3;
            // Built along +Z for the sizes without their signs, then mirrored when the length is negative.
            float span = Mathf.Abs(length);
            float radiusA = Mathf.Abs(_radiusA);
            float radiusB = Mathf.Abs(_radiusB);
            Vector3 centerB = new(0f, 0f, span);
            Hull.Tangent(radiusA, radiusB, span, out float sine, out float cosine);

            Span<Vector3> ringA = positions.Slice(0, segmentCount);
            Span<Vector3> ringB = positions.Slice(segmentCount, segmentCount);
            Span<Vector3> capA = positions.Slice(segmentCount * 2, capVertices);
            Span<Vector3> capB = positions.Slice(segmentCount * 2 + capVertices, capVertices);
            WriteRing(ringA, Vector3.zero, radiusA, sine, cosine);
            WriteRing(ringB, centerB, radiusB, sine, cosine);
            WriteCap(capA, Vector3.zero, radiusA, Vector3.back, Mathf.Atan2(cosine, sine));
            WriteCap(capB, centerB, radiusB, Vector3.forward, Mathf.Atan2(cosine, -sine));

            if (cosine == 0f)
            {
                // One sphere holds the other: the bigger one's arcs are full circles through the point where its ring
                // collapsed, and everything of the smaller one folds onto that point.
                if (sine < 0f)
                {
                    ringB.Fill(ringA[0]);
                    capB.Fill(ringA[0]);
                }
                else
                {
                    ringA.Fill(ringB[0]);
                    capA.Fill(ringB[0]);
                }
            }
            if (length < 0f)
            {
                for (int i = 0; i < positions.Length; i++)
                {
                    positions[i].z = -positions[i].z;
                }
            }
        }

        protected override void ScaleCrossSection(float factor)
        {
            _radiusA *= factor;
            _radiusB *= factor;
        }

        private static int CountVertices(int segmentCount)
        {
            // Two rings, and two caps whose arcs share the ring points and each cap's pole.
            return segmentCount * 4 - 6;
        }

        /// <summary>
        /// Writes the ring where the outline touches a sphere. It starts on +Y and passes +X a quarter turn later.
        /// </summary>
        private static void WriteRing(Span<Vector3> ring, Vector3 center, float radius, float sine, float cosine)
        {
            float ringRadius = radius * cosine;
            Ring.Write(
                ring,
                center + new Vector3(0f, 0f, -radius * sine),
                new Vector3(0f, ringRadius, 0f),
                new Vector3(ringRadius, 0f, 0f));
        }

        /// <summary>
        /// Writes the inside points of a cap's two arcs, which run over the pole from angle <paramref name="edge"/> on
        /// one side of the ring to -<paramref name="edge"/> on the other: first the arc through +Y and -Y, which holds
        /// the pole, then the arc through +X and -X without it.
        /// </summary>
        private static void WriteCap(Span<Vector3> cap, Vector3 center, float radius, Vector3 pole, float edge)
        {
            int half = (cap.Length + 3) / 2;
            int quarter = half / 2;
            int vertex = 0;
            for (int step = 1; step < half; step++)
            {
                cap[vertex++] = Hull.ArcPoint(center, radius, Vector3.up, pole, edge * (1f - 2f * step / half));
            }
            for (int step = 1; step < half; step++)
            {
                if (step != quarter)
                {
                    cap[vertex++] = Hull.ArcPoint(center, radius, Vector3.right, pole, edge * (1f - 2f * step / half));
                }
            }
        }

        private static int[] BuildEdges(int segmentCount)
        {
            int[] pattern = new int[(segmentCount * 4 + SideLineCount) * 2];
            int cursor = 0;
            Ring.AddEdges(pattern, ref cursor, 0, segmentCount);
            Ring.AddEdges(pattern, ref cursor, segmentCount, segmentCount);
            // The side lines join the rings at their quarter points, which is why the segment count must be a multiple of 4.
            for (int line = 0; line < SideLineCount; line++)
            {
                int vertex = line * segmentCount / SideLineCount;
                pattern[cursor++] = vertex;
                pattern[cursor++] = segmentCount + vertex;
            }
            int capVertices = segmentCount - 3;
            AddCapEdges(pattern, ref cursor, 0, segmentCount * 2, segmentCount);
            AddCapEdges(pattern, ref cursor, segmentCount, segmentCount * 2 + capVertices, segmentCount);
            return pattern;
        }

        /// <summary>Adds the edges of a cap's two arcs, in the order <see cref="WriteCap"/> writes their points.</summary>
        private static void AddCapEdges(int[] pattern, ref int cursor, int ring, int inside, int segmentCount)
        {
            int half = segmentCount / 2;
            int quarter = segmentCount / 4;
            int pole = inside + quarter - 1;

            // From the ring's +Y point over the pole to its -Y point.
            int previous = ring;
            for (int step = 1; step < half; step++)
            {
                pattern[cursor++] = previous;
                pattern[cursor++] = inside;
                previous = inside++;
            }
            pattern[cursor++] = previous;
            pattern[cursor++] = ring + half;

            // From the ring's +X point over the shared pole to its -X point.
            previous = ring + quarter;
            for (int step = 1; step < half; step++)
            {
                int current = step == quarter ? pole : inside++;
                pattern[cursor++] = previous;
                pattern[cursor++] = current;
                previous = current;
            }
            pattern[cursor++] = previous;
            pattern[cursor++] = ring + quarter * 3;
        }
    }
}
