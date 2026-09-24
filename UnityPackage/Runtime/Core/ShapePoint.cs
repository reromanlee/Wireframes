using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>One point of a point shape: its position relative to its own bone, its color and that bone's slot.</summary>
    internal struct ShapePoint
    {
        internal Vector3 LocalPosition;
        internal Color Color;
        internal Transform Bone;
        internal int BoneSlot;

        /// <summary>A white point in world space.</summary>
        internal ShapePoint(Vector3 localPosition)
        {
            LocalPosition = localPosition;
            Color = Color.white;
            Bone = null;
            BoneSlot = 0;
        }
    }
}
