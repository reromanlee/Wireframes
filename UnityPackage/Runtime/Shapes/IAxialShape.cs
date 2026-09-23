using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// A long shape that runs from end A, at <see cref="IRigidShape.LocalPosition"/>, along the +Z axis of its rotation
    /// to end B.
    /// </summary>
    public interface IAxialShape : IRigidShape
    {
        /// <summary>Distance from end A to end B, in the bone's units.</summary>
        float Length { get; set; }

        /// <summary>
        /// End B relative to <see cref="IRigidShape.Bone"/>, or in world space without a bone. Setting it keeps end A,
        /// turns the axis toward the new point by the smallest rotation, which keeps the spin around the axis as far
        /// as it can, and sets <see cref="Length"/>.
        /// </summary>
        Vector3 LocalEnd { get; set; }

        /// <summary>End B in world space, converted through the bone's current pose. Setting it works like <see cref="LocalEnd"/>.</summary>
        Vector3 WorldEnd { get; set; }
    }
}
