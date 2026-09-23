using UnityEngine;

namespace reromanlee.Wireframes
{
    public partial interface ILineContainer
    {
        /// <summary>
        /// Creates a white ellipse around the world origin, lying flat in the XZ plane, 1 unit long along Z and
        /// 0.5 wide along X.
        /// </summary>
        IEllipse CreateEllipse();

        /// <summary>
        /// Creates a white ellipse whose long axis runs between two world positions, with <paramref name="radius"/> as
        /// its other semi-axis. It lies as flat as it can: its normal stays as close to world up as the tips allow.
        /// </summary>
        /// <param name="segments">Number of straight pieces the ellipse is drawn with, at least 3.</param>
        IEllipse CreateEllipse(Vector3 tipA, Vector3 tipB, float radius, int segments = Ring.DefaultSegments);

        /// <summary>
        /// Creates a white ellipse whose long axis runs between two world positions, with <paramref name="radius"/> as
        /// its other semi-axis, facing as close to <paramref name="normal"/> as the tips allow.
        /// </summary>
        /// <param name="segments">Number of straight pieces the ellipse is drawn with, at least 3.</param>
        IEllipse CreateEllipse(
            Vector3 tipA, Vector3 tipB, float radius, Vector3 normal, int segments = Ring.DefaultSegments);

        /// <summary>
        /// Creates a white ellipse that follows <paramref name="bone"/>, whose long axis runs between two positions in
        /// the bone's local space. It lies as flat in the bone's XZ plane as the tips allow.
        /// </summary>
        /// <param name="segments">Number of straight pieces the ellipse is drawn with, at least 3.</param>
        IEllipse CreateEllipse(
            Transform bone, Vector3 localTipA, Vector3 localTipB, float radius, int segments = Ring.DefaultSegments);

        /// <summary>Creates a white ellipse in world space, lying in the XZ plane of <paramref name="rotation"/>.</summary>
        /// <param name="radii">Semi-axes: x along the rotation's X axis and y along its Z axis.</param>
        /// <param name="segments">Number of straight pieces the ellipse is drawn with, at least 3.</param>
        IEllipse CreateEllipse(Vector3 center, Quaternion rotation, Vector2 radii, int segments = Ring.DefaultSegments);
    }
}
