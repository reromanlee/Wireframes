using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>Creates stars in a <see cref="WireframeContainer"/>.</summary>
    public static class StarFactory
    {
        private const float DefaultInnerRadius = 0.2f;
        private const int DefaultPoints = 5;

        /// <summary>
        /// Creates a white five-pointed star around the world origin, lying flat in the XZ plane, with an outer radius
        /// of 0.5 and an inner radius of 0.2.
        /// </summary>
        public static IStar CreateStar(this WireframeContainer container)
        {
            MeshProxy proxy = WireframeContainer.ProxyOf(container);
            return new Star(
                proxy, null, Vector3.zero, Quaternion.identity, DefaultInnerRadius, ShapeDefaults.Radius, DefaultPoints);
        }

        /// <summary>
        /// Creates a white star around a world position, lying flat in the world's XZ plane with its first point along
        /// +Z.
        /// </summary>
        /// <param name="points">Number of points, at least 3.</param>
        public static IStar CreateStar(
            this WireframeContainer container, Vector3 center, float innerRadius, float outerRadius, int points)
        {
            MeshProxy proxy = WireframeContainer.ProxyOf(container);
            return new Star(proxy, null, center, Quaternion.identity, innerRadius, outerRadius, points);
        }

        /// <summary>
        /// Creates a white star that follows <paramref name="bone"/>, around a position in the bone's local space and
        /// lying flat in the bone's XZ plane with its first point along the bone's +Z.
        /// </summary>
        /// <param name="points">Number of points, at least 3.</param>
        public static IStar CreateStar(
            this WireframeContainer container,
            Transform bone,
            Vector3 localCenter,
            float innerRadius,
            float outerRadius,
            int points)
        {
            MeshProxy proxy = WireframeContainer.ProxyOf(container);
            return new Star(proxy, bone, localCenter, Quaternion.identity, innerRadius, outerRadius, points);
        }

        /// <summary>
        /// Creates a white star in world space, lying in the XZ plane of <paramref name="rotation"/> with its first point
        /// along the rotation's +Z.
        /// </summary>
        /// <param name="points">Number of points, at least 3.</param>
        public static IStar CreateStar(
            this WireframeContainer container,
            Vector3 center,
            Quaternion rotation,
            float innerRadius,
            float outerRadius,
            int points)
        {
            MeshProxy proxy = WireframeContainer.ProxyOf(container);
            return new Star(proxy, null, center, rotation, innerRadius, outerRadius, points);
        }
    }
}
