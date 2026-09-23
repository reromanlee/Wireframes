using UnityEngine;

namespace reromanlee.Wireframes
{
    public partial interface ILineContainer
    {
        /// <summary>Creates a white cone with its tip at the world origin and a base of radius 0.5, 1 unit along +Z.</summary>
        ICone CreateCone();

        /// <summary>
        /// Creates a white cone from a tip to the center of its base, both world positions. Its spin around the axis
        /// keeps the cone's +Y as close to world up as it can.
        /// </summary>
        /// <param name="segments">Number of straight pieces the base ring is drawn with, a positive multiple of 4.</param>
        ICone CreateCone(Vector3 tip, Vector3 baseCenter, float radius, int segments = Ring.DefaultSegments);

        /// <summary>
        /// Creates a white cone that follows <paramref name="bone"/>, from a tip to the center of its base, both in the
        /// bone's local space. Its spin around the axis keeps the cone's +Y as close to the bone's up as it can.
        /// </summary>
        /// <param name="segments">Number of straight pieces the base ring is drawn with, a positive multiple of 4.</param>
        ICone CreateCone(
            Transform bone, Vector3 localTip, Vector3 localBaseCenter, float radius, int segments = Ring.DefaultSegments);

        /// <summary>
        /// Creates a white cone in world space with its tip at <paramref name="position"/> and its base
        /// <paramref name="length"/> along the +Z axis of <paramref name="rotation"/>.
        /// </summary>
        /// <param name="segments">Number of straight pieces the base ring is drawn with, a positive multiple of 4.</param>
        ICone CreateCone(
            Vector3 position, Quaternion rotation, float length, float radius, int segments = Ring.DefaultSegments);
    }
}
