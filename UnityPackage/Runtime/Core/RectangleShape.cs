using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// Base of rectangles, which lie flat in the XZ plane of their rotation around their position. It keeps the size
    /// and the corner accessors, so derived shapes only draw the outline.
    /// </summary>
    internal abstract class RectangleShape : RigidShape
    {
        private Vector2 _size;

        protected RectangleShape(
            MeshProxy proxy,
            int vertexCount,
            int[] edgePattern,
            Transform bone,
            Vector3 localCenter,
            Quaternion localRotation,
            Vector2 size) : base(proxy, vertexCount, edgePattern, bone, localCenter, localRotation)
        {
            _size = size;
        }

        public Vector2 Size
        {
            get
            {
                ThrowIfDisposed();
                return _size;
            }
            set
            {
                ThrowIfDisposed();
                _size = value;
                MarkDirty(DirtyFlags.Positions);
            }
        }

        public Vector3 LocalCornerA
        {
            get
            {
                ThrowIfDisposed();
                return LocalPosition - LocalRotation * HalfSize();
            }
            set
            {
                ThrowIfDisposed();
                Vector3 cornerB = LocalCornerB;
                SpanCorners(OntoPlane(value, cornerB), cornerB);
            }
        }

        public Vector3 LocalCornerB
        {
            get
            {
                ThrowIfDisposed();
                return LocalPosition + LocalRotation * HalfSize();
            }
            set
            {
                ThrowIfDisposed();
                Vector3 cornerA = LocalCornerA;
                SpanCorners(cornerA, OntoPlane(value, cornerA));
            }
        }

        public Vector3 WorldCornerA
        {
            get
            {
                ThrowIfDisposed();
                return ToWorld(Bone, LocalCornerA);
            }
            set
            {
                ThrowIfDisposed();
                LocalCornerA = ToLocal(Bone, value);
            }
        }

        public Vector3 WorldCornerB
        {
            get
            {
                ThrowIfDisposed();
                return ToWorld(Bone, LocalCornerB);
            }
            set
            {
                ThrowIfDisposed();
                LocalCornerB = ToLocal(Bone, value);
            }
        }

        protected override void ScaleSizes(float factor)
        {
            _size *= factor;
        }

        private Vector3 HalfSize()
        {
            return new Vector3(_size.x * 0.5f, 0f, _size.y * 0.5f);
        }

        /// <summary>
        /// Moves <paramref name="point"/> along the rectangle's normal into the plane through <paramref name="onPlane"/>.
        /// </summary>
        private Vector3 OntoPlane(Vector3 point, Vector3 onPlane)
        {
            Vector3 normal = LocalRotation * Vector3.up;
            return point - normal * Vector3.Dot(point - onPlane, normal);
        }

        /// <summary>Spans the rectangle between two local corners in its plane, keeping its rotation.</summary>
        private void SpanCorners(Vector3 cornerA, Vector3 cornerB)
        {
            Vector3 span = Quaternion.Inverse(LocalRotation) * (cornerB - cornerA);
            LocalPosition = (cornerA + cornerB) * 0.5f;
            _size = new Vector2(span.x, span.z);
        }
    }
}
