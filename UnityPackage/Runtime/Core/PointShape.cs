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
        protected PointShape(int pointCount, EdgeSource edgeSource) : base(pointCount, edgeSource)
        {
        }

        public Vector3 GetLocalPosition(int index)
        {
            EnsureUsable();
            return LivePoint(index).LocalPosition;
        }

        public void SetLocalPosition(int index, Vector3 position)
        {
            EnsureUsable();
            LivePoint(index).LocalPosition = position;
            MarkDirty(DirtyFlags.Positions);
        }

        public Vector3 GetWorldPosition(int index)
        {
            EnsureUsable();
            ref ShapePoint point = ref LivePoint(index);
            return ToWorld(point.Bone, point.LocalPosition);
        }

        public void SetWorldPosition(int index, Vector3 position)
        {
            EnsureUsable();
            ref ShapePoint point = ref LivePoint(index);
            point.LocalPosition = ToLocal(point.Bone, position);
            MarkDirty(DirtyFlags.Positions);
        }

        public Color GetColor(int index)
        {
            EnsureUsable();
            return Point(index).Color;
        }

        public void SetColor(int index, Color color)
        {
            EnsureUsable();
            Point(index).Color = color;
            MarkDirty(DirtyFlags.Colors);
        }

        public Transform GetBone(int index)
        {
            EnsureUsable();
            return LivePoint(index).Bone;
        }

        public void SetBone(int index, Transform bone)
        {
            EnsureUsable();
            bone = CheckBone(bone, nameof(bone));
            ref ShapePoint point = ref LivePoint(index);
            if (ReferenceEquals(bone, point.Bone))
            {
                return;
            }
            Vector3 worldPosition = ToWorld(point.Bone, point.LocalPosition);
            ReplaceBone(ref point.Bone, ref point.BoneSlot, bone);
            point.LocalPosition = ToLocal(point.Bone, worldPosition);
            MarkDirty(DirtyFlags.Positions | DirtyFlags.Bones);
        }

        public override void SetColor(Color color)
        {
            EnsureUsable();
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

        internal override void WriteBoneIndices(Span<float> boneIndices)
        {
            for (int i = 0; i < boneIndices.Length; i++)
            {
                boneIndices[i] = Point(i).BoneSlot;
            }
        }

        protected override void AcquireBones(BoneRegistry bones)
        {
            for (int i = 0; i < VertexCount; i++)
            {
                ref ShapePoint point = ref Point(i);
                // A bone destroyed while the shape was suspended left no pose to hold it by, so it resumes in world space.
                if (IsDestroyed(point.Bone))
                {
                    point.Bone = null;
                }
                point.BoneSlot = bones.Acquire(point.Bone);
            }
        }

        protected override void ReleaseBones(BoneRegistry bones)
        {
            for (int i = 0; i < VertexCount; i++)
            {
                bones.Release(Point(i).BoneSlot);
            }
        }

        /// <summary>
        /// Sets the bone of point <paramref name="index"/> while the shape is being built, keeping its local position.
        /// </summary>
        protected void InitializeBone(int index, Transform bone, string parameterName)
        {
            Point(index).Bone = CheckBone(bone, parameterName);
        }

        /// <summary>Storage of point <paramref name="index"/>.</summary>
        protected abstract ref ShapePoint Point(int index);

        /// <summary>
        /// Storage of point <paramref name="index"/>, switched to world space first if its bone was destroyed. Only the
        /// point asked for is checked, so members of long polylines don't scan every point.
        /// </summary>
        private ref ShapePoint LivePoint(int index)
        {
            ref ShapePoint point = ref Point(index);
            if (IsDestroyed(point.Bone))
            {
                // The slot still holds the bone's last pose, so the point keeps its world position.
                point.LocalPosition = Bones.MatrixOf(point.BoneSlot).MultiplyPoint3x4(point.LocalPosition);
                Bones.Release(point.BoneSlot);
                point.Bone = null;
                point.BoneSlot = BoneRegistry.WorldSlot;
                MarkDirty(DirtyFlags.Positions | DirtyFlags.Bones);
            }
            return ref point;
        }
    }
}
