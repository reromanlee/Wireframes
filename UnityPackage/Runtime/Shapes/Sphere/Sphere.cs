using System;
using UnityEngine;

namespace reromanlee.Wireframes
{
    internal sealed class Sphere : RigidShape, ISphere
    {
        private const int CircleCount = 3;
        private static readonly PatternCache Patterns = new(BuildEdges);

        private float _radius;

        internal Sphere(
            MeshProxy proxy,
            Transform bone,
            Vector3 localCenter,
            Quaternion localRotation,
            float radius,
            int segments)
            : base(proxy, Ring.CheckSegments(segments) * CircleCount, Patterns.Get(segments), bone, localCenter, localRotation)
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
                return VertexCount / CircleCount;
            }
        }

        protected override void WriteShape(Span<Vector3> positions)
        {
            int segments = positions.Length / CircleCount;
            Vector3 x = new(_radius, 0f, 0f);
            Vector3 y = new(0f, _radius, 0f);
            Vector3 z = new(0f, 0f, _radius);
            // One great circle in each plane of the shape's axes: XZ, XY and YZ.
            Ring.Write(positions.Slice(0, segments), Vector3.zero, z, x);
            Ring.Write(positions.Slice(segments, segments), Vector3.zero, y, x);
            Ring.Write(positions.Slice(segments * 2, segments), Vector3.zero, y, z);
        }

        protected override void ScaleSizes(float factor)
        {
            _radius *= factor;
        }

        private static int[] BuildEdges(int segments)
        {
            int[] pattern = new int[segments * CircleCount * 2];
            int cursor = 0;
            for (int circle = 0; circle < CircleCount; circle++)
            {
                Ring.AddEdges(pattern, ref cursor, circle * segments, segments);
            }
            return pattern;
        }
    }
}
