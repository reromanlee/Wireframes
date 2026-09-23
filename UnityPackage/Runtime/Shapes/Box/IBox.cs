using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// A wireframe box spanned by two opposite corners. It is axis-aligned in its bone's space and moves, rotates
    /// and scales with that bone.
    /// </summary>
    public interface IBox : IShape
    {
        /// <summary>Corner A relative to <see cref="Bone"/>, or in world space without a bone.</summary>
        Vector3 LocalCornerA { get; set; }

        /// <summary>Corner B relative to <see cref="Bone"/>, or in world space without a bone.</summary>
        Vector3 LocalCornerB { get; set; }

        /// <summary>Corner A in world space, converted through <see cref="Bone"/>'s current pose.</summary>
        Vector3 WorldCornerA { get; set; }

        /// <summary>Corner B in world space, converted through <see cref="Bone"/>'s current pose.</summary>
        Vector3 WorldCornerB { get; set; }

        /// <summary>
        /// Transform the box follows, or null for world space. Changing it keeps both corners' world positions and
        /// re-aligns the box to the new bone's axes. If the bone is destroyed, the box stays where it was and becomes
        /// world space.
        /// </summary>
        Transform Bone { get; set; }
    }
}
