using System;
using UnityEngine;

namespace reromanlee.Wireframes
{
    internal sealed class Star : RigidShape, IStar
    {
        private float _innerRadius;
        private float _outerRadius;

        internal Star(
            MeshProxy proxy,
            Transform bone,
            Vector3 localCenter,
            Quaternion localRotation,
            float innerRadius,
            float outerRadius,
            int points)
            : base(proxy, CountVertices(points), Ring.Patterns.Get(points * 2), bone, localCenter, localRotation)
        {
            _innerRadius = innerRadius;
            _outerRadius = outerRadius;
        }

        public float InnerRadius
        {
            get
            {
                ThrowIfDisposed();
                return _innerRadius;
            }
            set
            {
                ThrowIfDisposed();
                _innerRadius = value;
                MarkDirty(DirtyFlags.Positions);
            }
        }

        public float OuterRadius
        {
            get
            {
                ThrowIfDisposed();
                return _outerRadius;
            }
            set
            {
                ThrowIfDisposed();
                _outerRadius = value;
                MarkDirty(DirtyFlags.Positions);
            }
        }

        public int PointCount
        {
            get
            {
                ThrowIfDisposed();
                return VertexCount / 2;
            }
        }

        protected override void WriteShape(Span<Vector3> positions)
        {
            // A ring that alternates between the tips and the corners between them, starting with a tip on +Z.
            Vector2[] circle = Ring.UnitCircle(positions.Length);
            for (int i = 0; i < positions.Length; i++)
            {
                float radius = i % 2 == 0 ? _outerRadius : _innerRadius;
                positions[i] = new Vector3(circle[i].y * radius, 0f, circle[i].x * radius);
            }
        }

        protected override void ScaleSizes(float factor)
        {
            _innerRadius *= factor;
            _outerRadius *= factor;
        }

        private static int CountVertices(int points)
        {
            if (points < 3)
            {
                throw new ArgumentOutOfRangeException(nameof(points), points, "A star needs at least 3 points.");
            }
            return points * 2;
        }
    }
}
