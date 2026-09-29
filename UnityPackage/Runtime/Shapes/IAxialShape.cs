using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// A long shape that runs from end A, at <see cref="IRigidShape.LocalPosition"/>, along the +Z axis of its rotation
    /// to end B. Setting one end keeps the other where it is, while setting the position moves the whole shape.
    /// </summary>
    public interface IAxialShape : IRigidShape
    {
        /// <summary>Distance from end A to end B, in the bone's units.</summary>
        float Length { get; set; }

        /// <summary>
        /// End A relative to <see cref="IRigidShape.Bone"/>, or in world space without a bone; the same point as
        /// <see cref="IRigidShape.LocalPosition"/>. Setting it keeps end B where it is: the axis turns by the smallest
        /// rotation and <see cref="Length"/> follows.
        /// </summary>
        Vector3 LocalEndA { get; set; }

        /// <summary>
        /// End B relative to <see cref="IRigidShape.Bone"/>, or in world space without a bone. Setting it keeps end A,
        /// turns the axis toward the new point by the smallest rotation, which keeps the spin around the axis as far
        /// as it can, and sets <see cref="Length"/>.
        /// </summary>
        Vector3 LocalEndB { get; set; }

        /// <summary>End A in world space, converted through the bone's current pose. Setting it works like <see cref="LocalEndA"/>.</summary>
        Vector3 WorldEndA { get; set; }

        /// <summary>End B in world space, converted through the bone's current pose. Setting it works like <see cref="LocalEndB"/>.</summary>
        Vector3 WorldEndB { get; set; }
    }
}
