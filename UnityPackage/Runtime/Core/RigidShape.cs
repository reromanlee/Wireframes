using System;
using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// Base of shapes that move rigidly with one bone. Like a child Transform, the shape keeps a position and a rotation
    /// relative to that bone, so derived shapes only write their points along the shape's own axes.
    /// </summary>
    internal abstract class RigidShape : Shape, IRigidShape
    {
        private Vector3 _localPosition;
        private Quaternion _localRotation;
        private Color _color = Color.white;
        private Transform _bone;
        private int _boneSlot;

        protected RigidShape(
            MeshProxy proxy,
            int vertexCount,
            int[] edgePattern,
            Transform bone,
            Vector3 localPosition,
            Quaternion localRotation) : base(proxy, vertexCount, edgePattern)
        {
            ReplaceBone(ref _bone, ref _boneSlot, bone);
            _localPosition = localPosition;
            _localRotation = Quaternion.Normalize(localRotation);
        }

        public Vector3 LocalPosition
        {
            get
            {
                ThrowIfDisposed();
                return _localPosition;
            }
            set
            {
                ThrowIfDisposed();
                _localPosition = value;
                MarkDirty(DirtyFlags.Positions);
            }
        }

        public Vector3 WorldPosition
        {
            get
            {
                ThrowIfDisposed();
                return ToWorld(_bone, _localPosition);
            }
            set
            {
                ThrowIfDisposed();
                _localPosition = ToLocal(_bone, value);
                MarkDirty(DirtyFlags.Positions);
            }
        }

        public Quaternion LocalRotation
        {
            get
            {
                ThrowIfDisposed();
                return _localRotation;
            }
            set
            {
                ThrowIfDisposed();
                // Normalizing also turns a zero quaternion, such as default, into no rotation.
                _localRotation = Quaternion.Normalize(value);
                MarkDirty(DirtyFlags.Positions);
            }
        }

        public Quaternion WorldRotation
        {
            get
            {
                ThrowIfDisposed();
                return ToWorld(_bone, _localRotation);
            }
            set
            {
                ThrowIfDisposed();
                _localRotation = ToLocal(_bone, Quaternion.Normalize(value));
                MarkDirty(DirtyFlags.Positions);
            }
        }

        public Color Color
        {
            get
            {
                ThrowIfDisposed();
                return _color;
            }
            set
            {
                ThrowIfDisposed();
                _color = value;
                MarkDirty(DirtyFlags.Colors);
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
                Vector3 worldPosition = ToWorld(_bone, _localPosition);
                Quaternion worldRotation = ToWorld(_bone, _localRotation);
                float oldScale = UniformScale(_bone);
                ReplaceBone(ref _bone, ref _boneSlot, value);
                _localPosition = ToLocal(_bone, worldPosition);
                _localRotation = ToLocal(_bone, worldRotation);
                // Sizes are in bone units, so they are converted to keep the world size, like a reparented Transform.
                float newScale = UniformScale(_bone);
                if (oldScale > 0f && newScale > 0f && oldScale != newScale)
                {
                    ScaleSizes(oldScale / newScale);
                }
                MarkDirty(DirtyFlags.Positions | DirtyFlags.Bones);
            }
        }

        public override void SetColor(Color color)
        {
            Color = color;
        }

        internal sealed override void WritePositions(Span<Vector3> positions)
        {
            WriteShape(positions);
            for (int i = 0; i < positions.Length; i++)
            {
                positions[i] = _localPosition + _localRotation * positions[i];
            }
        }

        internal sealed override void WriteColors(Span<Color32> colors)
        {
            colors.Fill(_color);
        }

        internal sealed override void WriteBones(Span<uint> bones)
        {
            bones.Fill((uint)_boneSlot);
        }

        internal sealed override void OnBoneDestroyed(Transform bone)
        {
            // Reference comparison: a destroyed bone also compares equal to null, which means "no bone".
            if (ReferenceEquals(_bone, bone))
            {
                Bone = null;
            }
        }

        protected sealed override void ReleaseBones(BoneRegistry bones)
        {
            bones.Release(_boneSlot);
        }

        /// <summary>Writes the shape's points along its own axes, before its rotation and position are applied.</summary>
        protected abstract void WriteShape(Span<Vector3> positions);

        /// <summary>Multiplies every size of the shape by <paramref name="factor"/>.</summary>
        protected abstract void ScaleSizes(float factor);

        private static Quaternion ToWorld(Transform bone, Quaternion local)
        {
            return bone != null ? bone.rotation * local : local;
        }

        private static Quaternion ToLocal(Transform bone, Quaternion world)
        {
            return bone != null ? Quaternion.Inverse(bone.rotation) * world : world;
        }

        /// <summary>A bone's overall scale: the geometric mean of its lossy scale, so a uniform scale comes back as is.</summary>
        private static float UniformScale(Transform bone)
        {
            if (bone == null)
            {
                return 1f;
            }
            Vector3 scale = bone.lossyScale;
            return (float)Math.Pow(Math.Abs((double)scale.x * scale.y * scale.z), 1.0 / 3.0);
        }
    }
}
