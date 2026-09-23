using System;
using UnityEngine;

namespace reromanlee.Wireframes
{
    internal sealed class Box : Shape, IBox
    {
        private const int CornerCount = 8;

        // Corner i takes x from corner B when bit 0 is set, y when bit 1 is set and z when bit 2 is set,
        // so corner 0 is A, corner 7 is B, and every edge joins two corners that differ in one bit.
        private static readonly int[] EdgePattern =
        {
            0, 1, 2, 3, 4, 5, 6, 7,
            0, 2, 1, 3, 4, 6, 5, 7,
            0, 4, 1, 5, 2, 6, 3, 7
        };

        private Vector3 _localCornerA;
        private Vector3 _localCornerB;
        private Color _color = Color.white;
        private Transform _bone;
        private int _boneSlot;

        internal Box(MeshProxy proxy, Vector3 cornerA, Vector3 cornerB) : base(proxy, CornerCount, EdgePattern)
        {
            _localCornerA = cornerA;
            _localCornerB = cornerB;
        }

        public Vector3 LocalCornerA
        {
            get
            {
                ThrowIfDisposed();
                return _localCornerA;
            }
            set
            {
                ThrowIfDisposed();
                _localCornerA = value;
                MarkDirty(DirtyFlags.Positions);
            }
        }

        public Vector3 LocalCornerB
        {
            get
            {
                ThrowIfDisposed();
                return _localCornerB;
            }
            set
            {
                ThrowIfDisposed();
                _localCornerB = value;
                MarkDirty(DirtyFlags.Positions);
            }
        }

        public Vector3 WorldCornerA
        {
            get
            {
                ThrowIfDisposed();
                return ToWorld(_bone, _localCornerA);
            }
            set
            {
                ThrowIfDisposed();
                _localCornerA = ToLocal(_bone, value);
                MarkDirty(DirtyFlags.Positions);
            }
        }

        public Vector3 WorldCornerB
        {
            get
            {
                ThrowIfDisposed();
                return ToWorld(_bone, _localCornerB);
            }
            set
            {
                ThrowIfDisposed();
                _localCornerB = ToLocal(_bone, value);
                MarkDirty(DirtyFlags.Positions);
            }
        }

        public Transform Bone
        {
            get
            {
                ThrowIfDisposed();
                return _bone;
            }
            set
            {
                ThrowIfDisposed();
                Vector3 worldCornerA = ToWorld(_bone, _localCornerA);
                Vector3 worldCornerB = ToWorld(_bone, _localCornerB);
                ReplaceBone(ref _bone, ref _boneSlot, value);
                _localCornerA = ToLocal(_bone, worldCornerA);
                _localCornerB = ToLocal(_bone, worldCornerB);
                MarkDirty(DirtyFlags.Positions | DirtyFlags.Bones);
            }
        }

        public override void SetColor(Color color)
        {
            ThrowIfDisposed();
            _color = color;
            MarkDirty(DirtyFlags.Colors);
        }

        internal override void WritePositions(Span<Vector3> positions)
        {
            for (int i = 0; i < CornerCount; i++)
            {
                positions[i] = new Vector3(
                    (i & 1) == 0 ? _localCornerA.x : _localCornerB.x,
                    (i & 2) == 0 ? _localCornerA.y : _localCornerB.y,
                    (i & 4) == 0 ? _localCornerA.z : _localCornerB.z);
            }
        }

        internal override void WriteColors(Span<Color32> colors)
        {
            colors.Fill(_color);
        }

        internal override void WriteBones(Span<uint> bones)
        {
            bones.Fill((uint)_boneSlot);
        }

        internal override void OnBoneDestroyed(Transform bone)
        {
            // Reference comparison: a destroyed bone also compares equal to null, which means "no bone".
            if (ReferenceEquals(_bone, bone))
            {
                Bone = null;
            }
        }

        protected override void ReleaseBones(BoneRegistry bones)
        {
            bones.Release(_boneSlot);
        }
    }
}
