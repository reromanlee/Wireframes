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
            int segmentCount)
            : base(
                proxy,
                Ring.CheckQuarterSegmentCount(segmentCount) + TipCount,
                Patterns.Get(segmentCount),
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

        public int SegmentCount
        {
            get
            {
                ThrowIfDisposed();
                return VertexCount - TipCount;
            }
        }

        protected override void WriteShape(Span<Vector3> positions, float length)
        {
            int segmentCount = positions.Length - TipCount;
            // The base ring starts on +Y and passes +X a quarter turn later; the tip comes last.
            Ring.Write(
                positions.Slice(0, segmentCount),
                new Vector3(0f, 0f, length),
                new Vector3(0f, _radius, 0f),
                new Vector3(_radius, 0f, 0f));
            positions[segmentCount] = Vector3.zero;
        }

        protected override void ScaleCrossSection(float factor)
        {
            _radius *= factor;
        }

        private static int[] BuildEdges(int segmentCount)
        {
            int[] pattern = new int[(segmentCount + SideLineCount) * 2];
            int cursor = 0;
            Ring.AddEdges(pattern, ref cursor, 0, segmentCount);
            // The side lines meet the ring at its quarter points, which is why the segment count must be a multiple of 4.
            for (int line = 0; line < SideLineCount; line++)
            {
                pattern[cursor++] = segmentCount;
                pattern[cursor++] = line * segmentCount / SideLineCount;
            }
            return pattern;
        }
    }
}
