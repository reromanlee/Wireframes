using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// Turns the forms that Create methods accept into shape rotations. Along a shape's own axes, +Y is the normal of
    /// a flat shape and +Z is the axis of a long one.
    /// </summary>
    internal static class ShapeRotations
    {
        // Below this squared length a direction is too short to read.
        private const float MinSqrLength = 1e-10f;
        // Directions whose squared cross product is below this are too close to parallel to define a spin.
        private const float MinSqrSine = 1e-6f;

        /// <summary>
        /// Rotation that points +Z along <paramref name="axis"/> and keeps +Y as close to <paramref name="up"/> as it
        /// can. An axis along <paramref name="up"/> is reached by the smallest rotation from +Z instead, and a zero
        /// axis gives no rotation.
        /// </summary>
        internal static Quaternion FromAxis(Vector3 axis, Vector3 up)
        {
            if (axis.sqrMagnitude < MinSqrLength)
            {
                return Quaternion.identity;
            }
            if (Vector3.Cross(axis.normalized, up.normalized).sqrMagnitude < MinSqrSine)
            {
                return Quaternion.FromToRotation(Vector3.forward, axis);
            }
            return Quaternion.LookRotation(axis, up);
        }

        /// <summary>Rotation that turns +Y onto <paramref name="normal"/> by the smallest rotation; a zero normal gives no rotation.</summary>
        internal static Quaternion FromNormal(Vector3 normal)
        {
            if (normal.sqrMagnitude < MinSqrLength)
            {
                return Quaternion.identity;
            }
            return Quaternion.FromToRotation(Vector3.up, normal);
        }
    }
}
