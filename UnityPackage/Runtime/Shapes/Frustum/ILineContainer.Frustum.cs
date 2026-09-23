using UnityEngine;

namespace reromanlee.Wireframes
{
    public partial interface ILineContainer
    {
        /// <summary>
        /// Creates a white square frustum from the world origin, 1 unit along +Z, with corners 0.25 out at end A and
        /// 0.5 out at end B.
        /// </summary>
        IFrustum CreateFrustum();

        /// <summary>
        /// Creates a white frustum between the centers of its two polygons, both world positions. Its spin around the
        /// axis keeps the frustum's +Y as close to world up as it can.
        /// </summary>
        /// <param name="sides">Number of sides of each polygon, at least 3.</param>
        IFrustum CreateFrustum(Vector3 endA, Vector3 endB, float radiusA, float radiusB, int sides);

        /// <summary>
        /// Creates a white frustum that follows <paramref name="bone"/>, between the centers of its two polygons in the
        /// bone's local space. Its spin around the axis keeps the frustum's +Y as close to the bone's up as it can.
        /// </summary>
        /// <param name="sides">Number of sides of each polygon, at least 3.</param>
        IFrustum CreateFrustum(
            Transform bone, Vector3 localEndA, Vector3 localEndB, float radiusA, float radiusB, int sides);

        /// <summary>
        /// Creates a white frustum in world space that starts at <paramref name="position"/> and runs
        /// <paramref name="length"/> along the +Z axis of <paramref name="rotation"/>.
        /// </summary>
        /// <param name="sides">Number of sides of each polygon, at least 3.</param>
        IFrustum CreateFrustum(
            Vector3 position, Quaternion rotation, float length, float radiusA, float radiusB, int sides);
    }
}
