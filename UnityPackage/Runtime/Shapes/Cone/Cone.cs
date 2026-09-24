using System;
using UnityEngine;

namespace reromanlee.Wireframes
{
    internal sealed class Cone : AxialShape, ICone
    {
        private const int TipCount = 1;
        private const int SideLineCount = 4;
        private static readonly PatternCache Patterns = new(BuildEdges);

        private float _radius;

        internal Cone(
            MeshProxy proxy,
            Transform bone,
            Vector3 localPosition,
            Quaternion localRotation,
            float length,
            float radius,
            int segments)
            : base(
                proxy,
                Ring.CheckQuarterSegments(segments) + TipCount,
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
                return VertexCount - TipCount;
            }
        }

        protected override void WriteShape(Span<Vector3> positions, float length)
        {
            int segments = positions.Length - TipCount;
            // The base ring starts on +Y and passes +X a quarter turn later; the tip comes last.
            Ring.Write(
                positions.Slice(0, segments),
                new Vector3(0f, 0f, length),
                new Vector3(0f, _radius, 0f),
                new Vector3(_radius, 0f, 0f));
            positions[segments] = Vector3.zero;
        }

        protected override void ScaleCrossSection(float factor)
        {
            _radius *= factor;
        }

        private static int[] BuildEdges(int segments)
        {
            int[] pattern = new int[(segments + SideLineCount) * 2];
            int cursor = 0;
            Ring.AddEdges(pattern, ref cursor, 0, segments);
            // The side lines meet the ring at its quarter points, which is why segments must be a multiple of 4.
            for (int line = 0; line < SideLineCount; line++)
            {
                pattern[cursor++] = segments;
                pattern[cursor++] = line * segments / SideLineCount;
            }
            return pattern;
        }
    }
}
