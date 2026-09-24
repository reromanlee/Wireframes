using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// A shape that moves rigidly with one bone, like a child Transform: it has a position and a rotation relative to
    /// that bone, and its sizes are in the bone's units. Along the shape's own axes, which its rotation turns, +Y is the
    /// normal of a flat shape and +Z is the axis of a long one.
    /// </summary>
    public interface IRigidShape : IShape
    {
        /// <summary>Position relative to <see cref="Bone"/>, or in world space without a bone.</summary>
        Vector3 LocalPosition { get; set; }

        /// <summary>Position in world space, converted through <see cref="Bone"/>'s current pose.</summary>
        Vector3 WorldPosition { get; set; }

        /// <summary>
        /// Rotation relative to <see cref="Bone"/>, or in world space without a bone. It is normalized when set, and a
        /// zero quaternion such as <c>default</c> means no rotation.
        /// </summary>
        Quaternion LocalRotation { get; set; }

        /// <summary>Rotation in world space, converted through <see cref="Bone"/>'s current rotation.</summary>
        Quaternion WorldRotation { get; set; }

        /// <summary>Color of every vertex of the shape.</summary>
        Color Color { get; set; }

        /// <summary>
        /// Transform the shape follows, or null for world space. Changing it keeps the shape's world position, rotation
        /// and size, like reparenting a Transform: sizes are converted by the ratio of the two bones' scales, which is
        /// exact for uniformly scaled bones. If the bone is destroyed, the shape stays where it was and becomes world
        /// space.
        /// </summary>
        Transform Bone { get; set; }
    }
}
