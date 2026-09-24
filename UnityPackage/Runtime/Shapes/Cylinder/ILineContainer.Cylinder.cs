using UnityEngine;

namespace reromanlee.Wireframes
{
    public partial interface ILineContainer
    {
        /// <summary>Creates a white cylinder of radius 0.5 from the world origin, 1 unit along +Z.</summary>
        ICylinder CreateCylinder();

        /// <summary>
        /// Creates a white cylinder between two world positions. Its spin around the axis keeps the cylinder's +Y as
        /// close to world up as it can.
        /// </summary>
        /// <param name="segments">Number of straight pieces each ring is drawn with, a positive multiple of 4.</param>
        ICylinder CreateCylinder(Vector3 endA, Vector3 endB, float radius, int segments = Ring.DefaultSegments);

        /// <summary>
        /// Creates a white cylinder that follows <paramref name="bone"/>, between two positions in the bone's local
        /// space. Its spin around the axis keeps the cylinder's +Y as close to the bone's up as it can.
        /// </summary>
        /// <param name="segments">Number of straight pieces each ring is drawn with, a positive multiple of 4.</param>
        ICylinder CreateCylinder(
            Transform bone, Vector3 localEndA, Vector3 localEndB, float radius, int segments = Ring.DefaultSegments);

        /// <summary>
        /// Creates a white cylinder in world space that starts at <paramref name="position"/> and runs
        /// <paramref name="length"/> along the +Z axis of <paramref name="rotation"/>.
        /// </summary>
        /// <param name="segments">Number of straight pieces each ring is drawn with, a positive multiple of 4.</param>
        ICylinder CreateCylinder(
            Vector3 position, Quaternion rotation, float length, float radius, int segments = Ring.DefaultSegments);
    }
}
