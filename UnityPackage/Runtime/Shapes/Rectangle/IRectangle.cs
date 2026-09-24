using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// A rectangle around <see cref="IRigidShape.LocalPosition"/>, lying flat in the XZ plane of its rotation, with
    /// <see cref="Size"/> along its X and Z axes. Its two opposite corners can also be read and set directly.
    /// </summary>
    public interface IRectangle : IRigidShape
    {
        /// <summary>
        /// Size in the bone's units: x is the width along the rectangle's X axis and y the depth along its Z axis.
        /// Negative components swap the corners, which is what lets both corners come back exactly as they were set.
        /// </summary>
        Vector2 Size { get; set; }

        /// <summary>
        /// Corner A relative to <see cref="IRigidShape.Bone"/>, or in world space without a bone: the corner half the
        /// size back along X and Z. Setting it moves only this corner: corner B and the rotation stay, and a point off
        /// the rectangle's plane is moved onto it along the normal.
        /// </summary>
        Vector3 LocalCornerA { get; set; }

        /// <summary>
        /// Corner B relative to <see cref="IRigidShape.Bone"/>, or in world space without a bone: the corner half the
        /// size ahead along X and Z. Setting it moves only this corner: corner A and the rotation stay, and a point off
        /// the rectangle's plane is moved onto it along the normal.
        /// </summary>
        Vector3 LocalCornerB { get; set; }

        /// <summary>Corner A in world space, converted through the bone's current pose.</summary>
        Vector3 WorldCornerA { get; set; }

        /// <summary>Corner B in world space, converted through the bone's current pose.</summary>
        Vector3 WorldCornerB { get; set; }
    }
}
