using System;
using UnityEngine;

namespace reromanlee.Wireframes
{
    internal sealed class Line : Shape, ILine
    {
        private const int LineVertexCount = 2;
        private static readonly int[] EdgePattern = { 0, 1 };

        private Vector3 _localPositionA;
        private Vector3 _localPositionB;
        private Color _colorA = Color.white;
        private Color _colorB = Color.white;
        private Transform _boneA;
        private Transform _boneB;
        private int _boneSlotA;
        private int _boneSlotB;

        internal Line(MeshProxy proxy, Vector3 positionA, Vector3 positionB) : base(proxy, LineVertexCount, EdgePattern)
        {
            _localPositionA = positionA;
            _localPositionB = positionB;
        }

        public Vector3 LocalPositionA
        {
            get
            {
                ThrowIfDisposed();
                return _localPositionA;
            }
            set
            {
                ThrowIfDisposed();
                _localPositionA = value;
                MarkDirty(DirtyFlags.Positions);
            }
        }

        public Vector3 LocalPositionB
        {
            get
            {
                ThrowIfDisposed();
                return _localPositionB;
            }
            set
            {
                ThrowIfDisposed();
                _localPositionB = value;
                MarkDirty(DirtyFlags.Positions);
            }
        }

        public Vector3 WorldPositionA
        {
            get
            {
                ThrowIfDisposed();
                return ToWorld(_boneA, _localPositionA);
            }
            set
            {
                ThrowIfDisposed();
                _localPositionA = ToLocal(_boneA, value);
                MarkDirty(DirtyFlags.Positions);
            }
        }

        public Vector3 WorldPositionB
        {
            get
            {
                ThrowIfDisposed();
                return ToWorld(_boneB, _localPositionB);
            }
            set
            {
                ThrowIfDisposed();
                _localPositionB = ToLocal(_boneB, value);
                MarkDirty(DirtyFlags.Positions);
            }
        }

        public Color ColorA
        {
            get
            {
                ThrowIfDisposed();
                return _colorA;
            }
            set
            {
                ThrowIfDisposed();
                _colorA = value;
                MarkDirty(DirtyFlags.Colors);
            }
        }

        public Color ColorB
        {
            get
            {
                ThrowIfDisposed();
                return _colorB;
            }
            set
            {
                ThrowIfDisposed();
                _colorB = value;
                MarkDirty(DirtyFlags.Colors);
            }
        }

        public Transform BoneA
        {
            get
            {
                ThrowIfDisposed();
                return _boneA;
            }
            set
            {
                ThrowIfDisposed();
                Vector3 worldPosition = ToWorld(_boneA, _localPositionA);
                ReplaceBone(ref _boneA, ref _boneSlotA, value);
                _localPositionA = ToLocal(_boneA, worldPosition);
                MarkDirty(DirtyFlags.Positions | DirtyFlags.Bones);
            }
        }

        public Transform BoneB
        {
            get
            {
                ThrowIfDisposed();
                return _boneB;
            }
            set
            {
                ThrowIfDisposed();
                Vector3 worldPosition = ToWorld(_boneB, _localPositionB);
                ReplaceBone(ref _boneB, ref _boneSlotB, value);
                _localPositionB = ToLocal(_boneB, worldPosition);
                MarkDirty(DirtyFlags.Positions | DirtyFlags.Bones);
            }
        }

        public override void SetColor(Color color)
        {
            ThrowIfDisposed();
            _colorA = color;
            _colorB = color;
            MarkDirty(DirtyFlags.Colors);
        }

        internal override void WritePositions(Span<Vector3> positions)
        {
            positions[0] = _localPositionA;
            positions[1] = _localPositionB;
        }

        internal override void WriteColors(Span<Color32> colors)
        {
            colors[0] = _colorA;
            colors[1] = _colorB;
        }

        internal override void WriteBones(Span<uint> bones)
        {
            bones[0] = (uint)_boneSlotA;
            bones[1] = (uint)_boneSlotB;
        }

        internal override void OnBoneDestroyed(Transform bone)
        {
            // Reference comparison: a destroyed bone also compares equal to every unattached (null) endpoint.
            if (ReferenceEquals(_boneA, bone))
            {
                BoneA = null;
            }
            if (ReferenceEquals(_boneB, bone))
            {
                BoneB = null;
            }
        }

        protected override void ReleaseBones(BoneRegistry bones)
        {
            bones.Release(_boneSlotA);
            bones.Release(_boneSlotB);
        }
    }
}
