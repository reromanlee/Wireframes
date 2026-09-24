using System;
using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// Base of long shapes. End A sits at the shape's position and end B lies <see cref="Length"/> further along the
    /// shape's +Z axis, so derived shapes only describe what runs between the two ends.
    /// </summary>
    internal abstract class AxialShape : RigidShape, IAxialShape
    {
        private float _length;

        protected AxialShape(
            MeshProxy proxy,
            int vertexCount,
            int[] edgePattern,
            Transform bone,
            Vector3 localPosition,
            Quaternion localRotation,
            float length) : base(proxy, vertexCount, edgePattern, bone, localPosition, localRotation)
        {
            _length = length;
        }

        public float Length
        {
            get
            {
                ThrowIfDisposed();
                return _length;
            }
            set
            {
                ThrowIfDisposed();
                _length = value;
                MarkDirty(DirtyFlags.Positions);
            }
        }

        public Vector3 LocalEnd
        {
            get
            {
                ThrowIfDisposed();
                return LocalPosition + LocalRotation * new Vector3(0f, 0f, _length);
            }
            set
            {
                ThrowIfDisposed();
                Vector3 axis = value - LocalPosition;
                float length = axis.magnitude;
                // An end on top of end A has no direction, so the rotation stays as it is.
                if (length > Vector3.kEpsilon)
                {
                    // The smallest turn keeps the spin around the axis, and a flat shape's normal, as far as it can.
                    Quaternion rotation = LocalRotation;
                    LocalRotation = Quaternion.FromToRotation(rotation * Vector3.forward, axis) * rotation;
                }
                _length = length;
                MarkDirty(DirtyFlags.Positions);
            }
        }

        public Vector3 WorldEnd
        {
            get
            {
                ThrowIfDisposed();
                return ToWorld(Bone, LocalEnd);
            }
            set
            {
                ThrowIfDisposed();
                LocalEnd = ToLocal(Bone, value);
            }
        }

        protected sealed override void WriteShape(Span<Vector3> positions)
        {
            WriteShape(positions, _length);
        }

        protected sealed override void ScaleSizes(float factor)
        {
            _length *= factor;
            ScaleCrossSection(factor);
        }

        /// <summary>Writes the shape's points along its own axes, with end A at the origin and end B at (0, 0, length).</summary>
        protected abstract void WriteShape(Span<Vector3> positions, float length);

        /// <summary>Multiplies the shape's sizes across its axis by <paramref name="factor"/>.</summary>
        protected abstract void ScaleCrossSection(float factor);
    }
}
