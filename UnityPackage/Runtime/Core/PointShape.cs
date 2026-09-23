using System;
using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// Base of shapes made of points joined by edges, where every point follows its own bone. Points can go anywhere
    /// without breaking the shape, which is what lets each one have a bone, unlike rigid shapes.
    /// </summary>
    internal abstract class PointShape : Shape
    {
        protected PointShape(MeshProxy proxy, int pointCount, int[] edgePattern) : base(proxy, pointCount, edgePattern)
        {
        }

        public Vector3 GetLocalPosition(int index)
        {
            ThrowIfDisposed();
            return Point(index).LocalPosition;
        }

        public void SetLocalPosition(int index, Vector3 position)
        {
            ThrowIfDisposed();
            Point(index).LocalPosition = position;
            MarkDirty(DirtyFlags.Positions);
        }

        public Vector3 GetWorldPosition(int index)
        {
            ThrowIfDisposed();
            ref ShapePoint point = ref Point(index);
            return ToWorld(point.Bone, point.LocalPosition);
        }

        public void SetWorldPosition(int index, Vector3 position)
        {
            ThrowIfDisposed();
            ref ShapePoint point = ref Point(index);
            point.LocalPosition = ToLocal(point.Bone, position);
            MarkDirty(DirtyFlags.Positions);
        }

        public Color GetColor(int index)
        {
            ThrowIfDisposed();
            return Point(index).Color;
        }

        public void SetColor(int index, Color color)
        {
            ThrowIfDisposed();
            Point(index).Color = color;
            MarkDirty(DirtyFlags.Colors);
        }

        public Transform GetBone(int index)
        {
            ThrowIfDisposed();
            return Point(index).Bone;
        }

        public void SetBone(int index, Transform bone)
        {
            ThrowIfDisposed();
            ref ShapePoint point = ref Point(index);
            Vector3 worldPosition = ToWorld(point.Bone, point.LocalPosition);
            ReplaceBone(ref point.Bone, ref point.BoneSlot, bone);
            point.LocalPosition = ToLocal(point.Bone, worldPosition);
            MarkDirty(DirtyFlags.Positions | DirtyFlags.Bones);
        }

        public override void SetColor(Color color)
        {
            ThrowIfDisposed();
            for (int i = 0; i < VertexCount; i++)
            {
                Point(i).Color = color;
            }
            MarkDirty(DirtyFlags.Colors);
        }

        internal override void WritePositions(Span<Vector3> positions)
        {
            for (int i = 0; i < positions.Length; i++)
            {
                positions[i] = Point(i).LocalPosition;
            }
        }

        internal override void WriteColors(Span<Color32> colors)
        {
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = Point(i).Color;
            }
        }

        internal override void WriteBones(Span<uint> bones)
        {
            for (int i = 0; i < bones.Length; i++)
            {
                bones[i] = (uint)Point(i).BoneSlot;
            }
        }

        internal override void OnBoneDestroyed(Transform bone)
        {
            for (int i = 0; i < VertexCount; i++)
            {
                // Reference comparison: a destroyed bone also compares equal to every unattached (null) point.
                if (ReferenceEquals(Point(i).Bone, bone))
                {
                    SetBone(i, null);
                }
            }
        }

        protected override void ReleaseBones(BoneRegistry bones)
        {
            for (int i = 0; i < VertexCount; i++)
            {
                bones.Release(Point(i).BoneSlot);
            }
        }

        /// <summary>Attaches point <paramref name="index"/> to <paramref name="bone"/>, keeping its local position.</summary>
        protected void AttachPoint(int index, Transform bone)
        {
            ref ShapePoint point = ref Point(index);
            ReplaceBone(ref point.Bone, ref point.BoneSlot, bone);
        }

        /// <summary>Storage of point <paramref name="index"/>.</summary>
        protected abstract ref ShapePoint Point(int index);
    }
}
