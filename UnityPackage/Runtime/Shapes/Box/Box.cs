using System;
using UnityEngine;

namespace reromanlee.Wireframes
{
    internal sealed class Box : RigidShape, IBox
    {
        private const int CornerCount = 8;

        // Corner i sits on the + side of x when bit 0 is set, of y when bit 1 is set and of z when bit 2 is set,
        // so corner 0 is A, corner 7 is B, and every edge joins two corners that differ in one bit.
        private static readonly int[] EdgePattern =
        {
            0, 1, 2, 3, 4, 5, 6, 7,
            0, 2, 1, 3, 4, 6, 5, 7,
            0, 4, 1, 5, 2, 6, 3, 7
        };

        private Vector3 _size;

        internal Box(MeshProxy proxy, Transform bone, Vector3 localCenter, Quaternion localRotation, Vector3 size)
            : base(proxy, CornerCount, EdgePattern, bone, localCenter, localRotation)
        {
            _size = size;
        }

        public Vector3 Size
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
                return LocalPosition - LocalRotation * (_size * 0.5f);
            }
            set
            {
                ThrowIfDisposed();
                SetCorners(value, LocalCornerB);
            }
        }

        public Vector3 LocalCornerB
        {
            get
            {
                ThrowIfDisposed();
                return LocalPosition + LocalRotation * (_size * 0.5f);
            }
            set
            {
                ThrowIfDisposed();
                SetCorners(LocalCornerA, value);
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

        protected override void WriteShape(Span<Vector3> positions)
        {
            Vector3 half = _size * 0.5f;
            for (int i = 0; i < CornerCount; i++)
            {
                positions[i] = new Vector3(
                    (i & 1) == 0 ? -half.x : half.x,
                    (i & 2) == 0 ? -half.y : half.y,
                    (i & 4) == 0 ? -half.z : half.z);
            }
        }

        protected override void ScaleSizes(float factor)
        {
            _size *= factor;
        }

        /// <summary>Spans the box between two local corners, keeping its rotation.</summary>
        private void SetCorners(Vector3 cornerA, Vector3 cornerB)
        {
            LocalPosition = (cornerA + cornerB) * 0.5f;
            _size = Quaternion.Inverse(LocalRotation) * (cornerB - cornerA);
        }
    }
}
