using UnityEngine;

namespace reromanlee.Wireframes
{
    public partial interface ILineContainer
    {
        /// <summary>
        /// Creates a white ellipsoid around the world origin, 1 unit long along Z and 0.5 wide along X and Y.
        /// </summary>
        IEllipsoid CreateEllipsoid();

        /// <summary>
        /// Creates a white ellipsoid whose long axis runs between two world positions, with <paramref name="radius"/>
        /// as its other two semi-axes. Its +Y stays as close to world up as the tips allow.
        /// </summary>
        /// <param name="segments">Number of straight pieces each ellipse is drawn with, at least 3.</param>
        IEllipsoid CreateEllipsoid(Vector3 tipA, Vector3 tipB, float radius, int segments = Ring.DefaultSegments);

        /// <summary>
        /// Creates a white ellipsoid that follows <paramref name="bone"/>, whose long axis runs between two positions in
        /// the bone's local space, with <paramref name="radius"/> as its other two semi-axes.
        /// </summary>
        /// <param name="segments">Number of straight pieces each ellipse is drawn with, at least 3.</param>
        IEllipsoid CreateEllipsoid(
            Transform bone, Vector3 localTipA, Vector3 localTipB, float radius, int segments = Ring.DefaultSegments);

        /// <summary>Creates a white ellipsoid in world space, turned by <paramref name="rotation"/>.</summary>
        /// <param name="radii">Semi-axes along the rotation's X, Y and Z axes.</param>
        /// <param name="segments">Number of straight pieces each ellipse is drawn with, at least 3.</param>
        IEllipsoid CreateEllipsoid(
            Vector3 center, Quaternion rotation, Vector3 radii, int segments = Ring.DefaultSegments);
    }
}
