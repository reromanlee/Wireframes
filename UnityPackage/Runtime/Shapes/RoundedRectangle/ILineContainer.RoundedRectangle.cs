using UnityEngine;

namespace reromanlee.Wireframes
{
    public partial interface ILineContainer
    {
        /// <summary>
        /// Creates a white 1 by 1 rectangle with corners of radius 0.25 around the world origin, lying flat in the XZ
        /// plane.
        /// </summary>
        IRoundedRectangle CreateRoundedRectangle();

        /// <summary>
        /// Creates a white, world-aligned rounded rectangle lying flat in the XZ plane, spanned by two opposite
        /// world-space corners. If the corners differ in height, the rectangle lies halfway between them.
        /// </summary>
        /// <param name="segments">
        /// Number of straight pieces a full circle of the corners is drawn with, a positive multiple of 4.
        /// </param>
        IRoundedRectangle CreateRoundedRectangle(
            Vector3 cornerA, Vector3 cornerB, float cornerRadius, int segments = Ring.DefaultSegments);

        /// <summary>
        /// Creates a white rounded rectangle that follows <paramref name="bone"/>, lying flat in the bone's XZ plane
        /// and spanned by two opposite corners in the bone's local space.
        /// </summary>
        /// <param name="segments">
        /// Number of straight pieces a full circle of the corners is drawn with, a positive multiple of 4.
        /// </param>
        IRoundedRectangle CreateRoundedRectangle(
            Transform bone,
            Vector3 localCornerA,
            Vector3 localCornerB,
            float cornerRadius,
            int segments = Ring.DefaultSegments);

        /// <summary>
        /// Creates a white rounded rectangle in world space, lying in the XZ plane of <paramref name="rotation"/>.
        /// </summary>
        /// <param name="segments">
        /// Number of straight pieces a full circle of the corners is drawn with, a positive multiple of 4.
        /// </param>
        IRoundedRectangle CreateRoundedRectangle(
            Vector3 center, Quaternion rotation, Vector2 size, float cornerRadius, int segments = Ring.DefaultSegments);
    }
}
