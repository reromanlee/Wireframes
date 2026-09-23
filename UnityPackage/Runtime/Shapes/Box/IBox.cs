using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// A wireframe box centered on <see cref="IRigidShape.LocalPosition"/>, turned by
    /// <see cref="IRigidShape.LocalRotation"/>, with <see cref="Size"/> along its own axes. Its two opposite corners
    /// can also be read and set directly.
    /// </summary>
    public interface IBox : IRigidShape
    {
        /// <summary>
        /// Size along the box's own axes, in the bone's units. Negative components swap the corners, which is what lets
        /// both corners come back exactly as they were set.
        /// </summary>
        Vector3 Size { get; set; }

        /// <summary>
        /// Corner A relative to <see cref="IRigidShape.Bone"/>, or in world space without a bone: the corner half the
        /// size back along each of the box's axes. Setting it moves only this corner; corner B and the rotation stay.
        /// </summary>
        Vector3 LocalCornerA { get; set; }

        /// <summary>
        /// Corner B relative to <see cref="IRigidShape.Bone"/>, or in world space without a bone: the corner half the
        /// size ahead along each of the box's axes. Setting it moves only this corner; corner A and the rotation stay.
        /// </summary>
        Vector3 LocalCornerB { get; set; }

        /// <summary>Corner A in world space, converted through the bone's current pose.</summary>
        Vector3 WorldCornerA { get; set; }

        /// <summary>Corner B in world space, converted through the bone's current pose.</summary>
        Vector3 WorldCornerB { get; set; }
    }
}
