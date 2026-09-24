using System;
using UnityEngine;

namespace reromanlee.Wireframes
{
    internal sealed class Ellipse : RigidShape, IEllipse
    {
        private Vector2 _radii;

        internal Ellipse(
            MeshProxy proxy,
            Transform bone,
            Vector3 localCenter,
            Quaternion localRotation,
            Vector2 radii,
            int segments)
            : base(proxy, Ring.CheckSegments(segments), Ring.Patterns.Get(segments), bone, localCenter, localRotation)
        {
            _radii = radii;
        }

        public Vector2 Radii
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
                return VertexCount;
            }
        }

        protected override void WriteShape(Span<Vector3> positions)
        {
            // Starts on +Z and passes +X a quarter turn later, like a circle.
            Ring.Write(positions, Vector3.zero, new Vector3(0f, 0f, _radii.y), new Vector3(_radii.x, 0f, 0f));
        }

        protected override void ScaleSizes(float factor)
        {
            _radii *= factor;
        }
    }
}
