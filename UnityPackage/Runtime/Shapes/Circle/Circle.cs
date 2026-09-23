using System;
using UnityEngine;

namespace reromanlee.Wireframes
{
    internal sealed class Circle : RigidShape, ICircle
    {
        private static readonly PatternCache Patterns = new(BuildEdges);

        private float _radius;

        internal Circle(
            MeshProxy proxy,
            Transform bone,
            Vector3 localCenter,
            Quaternion localRotation,
            float radius,
            int segments)
            : base(proxy, Ring.CheckSegments(segments), Patterns.Get(segments), bone, localCenter, localRotation)
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
                return VertexCount;
            }
        }

        protected override void WriteShape(Span<Vector3> positions)
        {
            // Starts on +Z and passes +X a quarter turn later.
            Ring.Write(positions, Vector3.zero, new Vector3(0f, 0f, _radius), new Vector3(_radius, 0f, 0f));
        }

        protected override void ScaleSizes(float factor)
        {
            _radius *= factor;
        }

        private static int[] BuildEdges(int segments)
        {
            int[] pattern = new int[segments * 2];
            int cursor = 0;
            Ring.AddEdges(pattern, ref cursor, 0, segments);
            return pattern;
        }
    }
}
