using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>Creates ellipsoids in a <see cref="WireframeContainer"/>.</summary>
    public static class EllipsoidFactory
    {
        /// <summary>
        /// Creates a white ellipsoid around the world origin, 1 unit long along Z and 0.5 wide along X and Y.
        /// </summary>
        public static IEllipsoid CreateEllipsoid(this WireframeContainer container)
        {
            MeshProxy proxy = WireframeContainer.ProxyOf(container);
            Vector3 radii = new(ShapeDefaults.Radius * 0.5f, ShapeDefaults.Radius * 0.5f, ShapeDefaults.Radius);
            return new Ellipsoid(proxy, null, Vector3.zero, Quaternion.identity, radii, Ring.DefaultSegmentCount);
        }

        /// <summary>
        /// Creates a white ellipsoid whose long axis runs between two world positions, with <paramref name="radius"/>
        /// as its other two semi-axes. Its +Y stays as close to world up as the tips allow.
        /// </summary>
        /// <param name="segmentCount">Number of straight pieces each ellipse is drawn with, at least 3.</param>
        public static IEllipsoid CreateEllipsoid(
            this WireframeContainer container,
            Vector3 tipA,
            Vector3 tipB,
            float radius,
            int segmentCount = Ring.DefaultSegmentCount)
        {
            MeshProxy proxy = WireframeContainer.ProxyOf(container);
            Quaternion rotation = ShapeRotations.FromAxis(tipB - tipA, Vector3.up);
            Vector3 radii = new(radius, radius, Vector3.Distance(tipA, tipB) * 0.5f);
            return new Ellipsoid(proxy, null, (tipA + tipB) * 0.5f, rotation, radii, segmentCount);
        }

        /// <summary>
        /// Creates a white ellipsoid that follows <paramref name="bone"/>, whose long axis runs between two positions in
        /// the bone's local space, with <paramref name="radius"/> as its other two semi-axes.
        /// </summary>
        /// <param name="segmentCount">Number of straight pieces each ellipse is drawn with, at least 3.</param>
        public static IEllipsoid CreateEllipsoid(
            this WireframeContainer container,
            Transform bone,
            Vector3 localTipA,
            Vector3 localTipB,
            float radius,
            int segmentCount = Ring.DefaultSegmentCount)
        {
            MeshProxy proxy = WireframeContainer.ProxyOf(container);
            Quaternion rotation = ShapeRotations.FromAxis(localTipB - localTipA, Vector3.up);
            Vector3 radii = new(radius, radius, Vector3.Distance(localTipA, localTipB) * 0.5f);
            return new Ellipsoid(proxy, bone, (localTipA + localTipB) * 0.5f, rotation, radii, segmentCount);
        }

        /// <summary>Creates a white ellipsoid in world space, turned by <paramref name="rotation"/>.</summary>
        /// <param name="radii">Semi-axes along the rotation's X, Y and Z axes.</param>
        /// <param name="segmentCount">Number of straight pieces each ellipse is drawn with, at least 3.</param>
        public static IEllipsoid CreateEllipsoid(
            this WireframeContainer container,
            Vector3 center,
            Quaternion rotation,
            Vector3 radii,
            int segmentCount = Ring.DefaultSegmentCount)
        {
            return new Ellipsoid(WireframeContainer.ProxyOf(container), null, center, rotation, radii, segmentCount);
        }
    }
}
