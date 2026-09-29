using System;
using UnityEngine;

namespace reromanlee.Wireframes
{
    internal sealed class Star : RigidShape, IStar
    {
        internal const int MaxPointCount = Ring.MaxSegmentCount / 2;

        private float _innerRadius;
        private float _outerRadius;

        internal Star(
            Transform bone,
            Vector3 localCenter,
            Quaternion localRotation,
            float innerRadius,
            float outerRadius,
            int pointCount)
            : base(CountVertices(pointCount), Ring.Patterns.Get(pointCount * 2), bone, localCenter, localRotation)
        {
            _innerRadius = innerRadius;
            _outerRadius = outerRadius;
        }

        public float InnerRadius
        {
            get
            {
                EnsureUsable();
                return _innerRadius;
            }
            set
            {
                EnsureUsable();
                _innerRadius = value;
                MarkDirty(DirtyFlags.Positions);
            }
        }

        public float OuterRadius
        {
            get
            {
                EnsureUsable();
                return _outerRadius;
            }
            set
            {
                EnsureUsable();
                _outerRadius = value;
                MarkDirty(DirtyFlags.Positions);
            }
        }

        public int PointCount
        {
            get
            {
                EnsureUsable();
                return VertexCount / 2;
            }
        }

        protected override void WriteShape(Span<Vector3> positions)
        {
            // A ring that alternates between the tips and the corners between them, starting with a tip on +Z.
            CirclePoints circle = new(positions.Length);
            for (int i = 0; i < positions.Length; i++)
            {
                Vector2 point = circle.Next();
                float radius = i % 2 == 0 ? _outerRadius : _innerRadius;
                positions[i] = new Vector3(point.y * radius, 0f, point.x * radius);
            }
        }

        protected override void ScaleSizes(float factor)
        {
            _innerRadius *= factor;
            _outerRadius *= factor;
        }

        private static int CountVertices(int pointCount)
        {
            // Its tips and corners make one ring.
            if (pointCount < 3 || pointCount > MaxPointCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(pointCount), pointCount, $"A star needs 3 to {MaxPointCount} points.");
            }
            return pointCount * 2;
        }
    }
}
