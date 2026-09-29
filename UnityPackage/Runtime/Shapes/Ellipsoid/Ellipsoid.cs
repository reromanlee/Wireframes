using System;
using UnityEngine;

namespace reromanlee.Wireframes
{
    internal sealed class Ellipsoid : RigidShape, IEllipsoid
    {
        private Vector3 _radii;

        internal Ellipsoid(
            Transform bone,
            Vector3 localCenter,
            Quaternion localRotation,
            Vector3 radii,
            int segmentCount)
            : base(
                Ring.CheckSegmentCount(segmentCount) * AxisRings.RingCount,
                AxisRings.Patterns.Get(segmentCount),
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
                EnsureUsable();
                return _radii;
            }
            set
            {
                EnsureUsable();
                _radii = value;
                MarkDirty(DirtyFlags.Positions);
            }
        }

        public int SegmentCount
        {
            get
            {
                EnsureUsable();
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
