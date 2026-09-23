using System;
using UnityEngine;

namespace reromanlee.Wireframes
{
    internal sealed class Cylinder : AxialShape, ICylinder
    {
        private const int RingCount = 2;
        private const int SideLineCount = 4;
        private static readonly PatternCache Patterns = new(BuildEdges);

        private float _radius;

        internal Cylinder(
            MeshProxy proxy,
            Transform bone,
            Vector3 localPosition,
            Quaternion localRotation,
            float length,
            float radius,
            int segments)
            : base(
                proxy,
                Ring.CheckQuarterSegments(segments) * RingCount,
                Patterns.Get(segments),
                bone,
                localPosition,
                localRotation,
                length)
        {
            _radius = radius;
        }

        public float Radius
        {
            get
            {
                ThrowIfDisposed();
                return _radius;
            }
            set
            {
                ThrowIfDisposed();
                _radius = value;
                MarkDirty(DirtyFlags.Positions);
            }
        }

        public int Segments
        {
            get
            {
                ThrowIfDisposed();
                return VertexCount / RingCount;
            }
        }

        protected override void WriteShape(Span<Vector3> positions, float length)
        {
            int segments = positions.Length / RingCount;
            // Each ring starts on +Y and passes +X a quarter turn later.
            Vector3 up = new(0f, _radius, 0f);
            Vector3 right = new(_radius, 0f, 0f);
            Ring.Write(positions.Slice(0, segments), Vector3.zero, up, right);
            Ring.Write(positions.Slice(segments, segments), new Vector3(0f, 0f, length), up, right);
        }

        protected override void ScaleCrossSection(float factor)
        {
            _radius *= factor;
        }

        private static int[] BuildEdges(int segments)
        {
            int[] pattern = new int[(segments * RingCount + SideLineCount) * 2];
            int cursor = 0;
            Ring.AddEdges(pattern, ref cursor, 0, segments);
            Ring.AddEdges(pattern, ref cursor, segments, segments);
            // The side lines join the rings at their quarter points, which is why segments must be a multiple of 4.
            for (int line = 0; line < SideLineCount; line++)
            {
                int vertex = line * segments / SideLineCount;
                pattern[cursor++] = vertex;
                pattern[cursor++] = segments + vertex;
            }
            return pattern;
        }
    }
}
