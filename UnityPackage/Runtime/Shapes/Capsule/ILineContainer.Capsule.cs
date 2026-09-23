using UnityEngine;

namespace reromanlee.Wireframes
{
    public partial interface ILineContainer
    {
        /// <summary>
        /// Creates a white capsule with spheres of radius 0.5 centered on the world origin and 1 unit along +Z.
        /// </summary>
        ICapsule CreateCapsule();

        /// <summary>
        /// Creates a white capsule around two spheres of the same radius, centered on two world positions, like the
        /// capsule of <c>Physics.CapsuleCast</c>. Its spin around the axis keeps the capsule's +Y as close to world up
        /// as it can.
        /// </summary>
        /// <param name="segments">Number of straight pieces each ring is drawn with, a positive multiple of 4.</param>
        ICapsule CreateCapsule(Vector3 centerA, Vector3 centerB, float radius, int segments = Ring.DefaultSegments);

        /// <summary>
        /// Creates a white capsule around two spheres centered on two world positions, each with its own radius. Its
        /// spin around the axis keeps the capsule's +Y as close to world up as it can.
        /// </summary>
        /// <param name="segments">Number of straight pieces each ring is drawn with, a positive multiple of 4.</param>
        ICapsule CreateCapsule(
            Vector3 centerA, Vector3 centerB, float radiusA, float radiusB, int segments = Ring.DefaultSegments);

        /// <summary>
        /// Creates a white capsule that follows <paramref name="bone"/>, around two spheres centered on positions in the
        /// bone's local space. Its spin around the axis keeps the capsule's +Y as close to the bone's up as it can.
        /// </summary>
        /// <param name="segments">Number of straight pieces each ring is drawn with, a positive multiple of 4.</param>
        ICapsule CreateCapsule(
            Transform bone,
            Vector3 localCenterA,
            Vector3 localCenterB,
            float radiusA,
            float radiusB,
            int segments = Ring.DefaultSegments);

        /// <summary>
        /// Creates a white capsule in world space whose first sphere is centered on <paramref name="position"/> and
        /// whose second sphere is centered <paramref name="length"/> along the +Z axis of <paramref name="rotation"/>.
        /// </summary>
        /// <param name="segments">Number of straight pieces each ring is drawn with, a positive multiple of 4.</param>
        ICapsule CreateCapsule(
            Vector3 position,
            Quaternion rotation,
            float length,
            float radiusA,
            float radiusB,
            int segments = Ring.DefaultSegments);
    }
}
