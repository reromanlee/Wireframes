using UnityEngine;

namespace reromanlee.Wireframes
{
    public partial interface ILineContainer
    {
        /// <summary>
        /// Creates a white five-pointed star around the world origin, lying flat in the XZ plane, with an outer radius
        /// of 0.5 and an inner radius of 0.2.
        /// </summary>
        IStar CreateStar();

        /// <summary>
        /// Creates a white star around a world position, lying flat in the world's XZ plane with its first point along
        /// +Z.
        /// </summary>
        /// <param name="points">Number of points, at least 3.</param>
        IStar CreateStar(Vector3 center, float innerRadius, float outerRadius, int points);

        /// <summary>
        /// Creates a white star that follows <paramref name="bone"/>, around a position in the bone's local space and
        /// lying flat in the bone's XZ plane with its first point along the bone's +Z.
        /// </summary>
        /// <param name="points">Number of points, at least 3.</param>
        IStar CreateStar(Transform bone, Vector3 localCenter, float innerRadius, float outerRadius, int points);

        /// <summary>
        /// Creates a white star in world space, lying in the XZ plane of <paramref name="rotation"/> with its first point
        /// along the rotation's +Z.
        /// </summary>
        /// <param name="points">Number of points, at least 3.</param>
        IStar CreateStar(Vector3 center, Quaternion rotation, float innerRadius, float outerRadius, int points);
    }
}
