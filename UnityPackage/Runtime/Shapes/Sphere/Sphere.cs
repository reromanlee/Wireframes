using System;
using UnityEngine;

namespace reromanlee.Wireframes
{
    internal sealed class Sphere : RigidShape, ISphere
    {
        private float _radius;

        internal Sphere(
            MeshProxy proxy,
            Transform bone,
            Vector3 localCenter,
            Quaternion localRotation,
            float radius,
            int segments)
            : base(
                proxy,
                Ring.CheckSegments(segments) * AxisRings.RingCount,
                AxisRings.Patterns.Get(segments),
                bone,
                localCenter,
                localRotation)
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
                return VertexCount / AxisRings.RingCount;
            }
        }

        protected override void WriteShape(Span<Vector3> positions)
        {
            AxisRings.Write(positions, new Vector3(_radius, _radius, _radius));
        }

        protected override void ScaleSizes(float factor)
        {
            _radius *= factor;
        }
    }
}
