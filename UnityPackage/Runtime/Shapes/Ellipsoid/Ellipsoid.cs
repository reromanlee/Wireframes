using System;
using UnityEngine;

namespace reromanlee.Wireframes
{
    internal sealed class Ellipsoid : RigidShape, IEllipsoid
    {
        private Vector3 _radii;

        internal Ellipsoid(
            MeshProxy proxy,
            Transform bone,
            Vector3 localCenter,
            Quaternion localRotation,
            Vector3 radii,
            int segments)
            : base(
                proxy,
                Ring.CheckSegments(segments) * AxisRings.RingCount,
                AxisRings.Patterns.Get(segments),
                bone,
                localCenter,
                localRotation)
        {
            _radii = radii;
        }

        public Vector3 Radii
        {
            get
            {
                ThrowIfDisposed();
                return _radii;
            }
            set
            {
                ThrowIfDisposed();
                _radii = value;
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
            AxisRings.Write(positions, _radii);
        }

        protected override void ScaleSizes(float factor)
        {
            _radii *= factor;
        }
    }
}
