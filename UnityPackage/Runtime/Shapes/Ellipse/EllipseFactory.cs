using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>Creates ellipses in a <see cref="WireframeContainer"/>.</summary>
    public static class EllipseFactory
    {
        /// <summary>
        /// Creates a white ellipse around the world origin, lying flat in the XZ plane, 1 unit long along Z and
        /// 0.5 wide along X.
        /// </summary>
        public static IEllipse CreateEllipse(this WireframeContainer container)
        {
            MeshProxy proxy = WireframeContainer.ProxyOf(container);
            Vector2 radii = new(ShapeDefaults.Radius * 0.5f, ShapeDefaults.Radius);
            return new Ellipse(proxy, null, Vector3.zero, Quaternion.identity, radii, Ring.DefaultSegmentCount);
        }

        /// <summary>
        /// Creates a white ellipse whose long axis runs between two world positions, with <paramref name="radius"/> as
        /// its other semi-axis. It lies as flat as it can: its normal stays as close to world up as the tips allow.
        /// </summary>
        /// <param name="segmentCount">Number of straight pieces the ellipse is drawn with, at least 3.</param>
        public static IEllipse CreateEllipse(
            this WireframeContainer container,
            Vector3 tipA,
            Vector3 tipB,
            float radius,
            int segmentCount = Ring.DefaultSegmentCount)
        {
            return container.CreateEllipse(tipA, tipB, radius, Vector3.up, segmentCount);
        }

        /// <summary>
        /// Creates a white ellipse whose long axis runs between two world positions, with <paramref name="radius"/> as
        /// its other semi-axis, facing as close to <paramref name="normal"/> as the tips allow.
        /// </summary>
        /// <param name="segmentCount">Number of straight pieces the ellipse is drawn with, at least 3.</param>
        public static IEllipse CreateEllipse(
            this WireframeContainer container,
            Vector3 tipA,
            Vector3 tipB,
            float radius,
            Vector3 normal,
            int segmentCount = Ring.DefaultSegmentCount)
        {
            MeshProxy proxy = WireframeContainer.ProxyOf(container);
            Quaternion rotation = ShapeRotations.FromAxis(tipB - tipA, normal);
            Vector2 radii = new(radius, Vector3.Distance(tipA, tipB) * 0.5f);
            return new Ellipse(proxy, null, (tipA + tipB) * 0.5f, rotation, radii, segmentCount);
        }

        /// <summary>
        /// Creates a white ellipse that follows <paramref name="bone"/>, whose long axis runs between two positions in
        /// the bone's local space. It lies as flat in the bone's XZ plane as the tips allow.
        /// </summary>
        /// <param name="segmentCount">Number of straight pieces the ellipse is drawn with, at least 3.</param>
        public static IEllipse CreateEllipse(
            this WireframeContainer container,
            Transform bone,
            Vector3 localTipA,
            Vector3 localTipB,
            float radius,
            int segmentCount = Ring.DefaultSegmentCount)
        {
            MeshProxy proxy = WireframeContainer.ProxyOf(container);
            Quaternion rotation = ShapeRotations.FromAxis(localTipB - localTipA, Vector3.up);
            Vector2 radii = new(radius, Vector3.Distance(localTipA, localTipB) * 0.5f);
            return new Ellipse(proxy, bone, (localTipA + localTipB) * 0.5f, rotation, radii, segmentCount);
        }

        /// <summary>Creates a white ellipse in world space, lying in the XZ plane of <paramref name="rotation"/>.</summary>
        /// <param name="radii">Semi-axes: x along the rotation's X axis and y along its Z axis.</param>
        /// <param name="segmentCount">Number of straight pieces the ellipse is drawn with, at least 3.</param>
        public static IEllipse CreateEllipse(
            this WireframeContainer container,
            Vector3 center,
            Quaternion rotation,
            Vector2 radii,
            int segmentCount = Ring.DefaultSegmentCount)
        {
            return new Ellipse(WireframeContainer.ProxyOf(container), null, center, rotation, radii, segmentCount);
        }
    }
}
