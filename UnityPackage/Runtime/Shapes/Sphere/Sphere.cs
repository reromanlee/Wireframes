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
            int segmentCount)
            : base(
                proxy,
                Ring.CheckSegmentCount(segmentCount) * AxisRings.RingCount,
                AxisRings.Patterns.Get(segmentCount),
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
                EnsureUsable();
                return _radius;
            }
            set
            {
                EnsureUsable();
                _radius = value;
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
            AxisRings.Write(positions, new Vector3(_radius, _radius, _radius));
        }

        protected override void ScaleSizes(float factor)
        {
            _radius *= factor;
        }
    }
}
