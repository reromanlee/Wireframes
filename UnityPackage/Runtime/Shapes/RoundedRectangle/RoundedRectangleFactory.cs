using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>Creates rounded rectangles in a <see cref="WireframeContainer"/>.</summary>
    public static class RoundedRectangleFactory
    {
        private const float DefaultCornerRadius = 0.25f;

        /// <summary>
        /// Creates a white 1 by 1 rectangle with corners of radius 0.25 around the world origin, lying flat in the XZ
        /// plane.
        /// </summary>
        public static IRoundedRectangle CreateRoundedRectangle(this WireframeContainer container)
        {
            MeshProxy proxy = WireframeContainer.ProxyOf(container);
            Vector2 size = Vector2.one * ShapeDefaults.Size;
            return new RoundedRectangle(
                proxy, null, Vector3.zero, Quaternion.identity, size, DefaultCornerRadius, Ring.DefaultSegmentCount);
        }

        /// <summary>
        /// Creates a white, world-aligned rounded rectangle lying flat in the XZ plane, spanned by two opposite
        /// world-space corners. If the corners differ in height, the rectangle lies halfway between them.
        /// </summary>
        /// <param name="segmentCount">
        /// Number of straight pieces a full circle of the corners is drawn with, a positive multiple of 4.
        /// </param>
        public static IRoundedRectangle CreateRoundedRectangle(
            this WireframeContainer container,
            Vector3 cornerA,
            Vector3 cornerB,
            float cornerRadius,
            int segmentCount = Ring.DefaultSegmentCount)
        {
            MeshProxy proxy = WireframeContainer.ProxyOf(container);
            Vector3 center = (cornerA + cornerB) * 0.5f;
            Vector2 size = RectangleShape.FlatSpan(cornerA, cornerB);
            return new RoundedRectangle(proxy, null, center, Quaternion.identity, size, cornerRadius, segmentCount);
        }

        /// <summary>
        /// Creates a white rounded rectangle that follows <paramref name="bone"/>, lying flat in the bone's XZ plane
        /// and spanned by two opposite corners in the bone's local space.
        /// </summary>
        /// <param name="segmentCount">
        /// Number of straight pieces a full circle of the corners is drawn with, a positive multiple of 4.
        /// </param>
        public static IRoundedRectangle CreateRoundedRectangle(
            this WireframeContainer container,
            Transform bone,
            Vector3 localCornerA,
            Vector3 localCornerB,
            float cornerRadius,
            int segmentCount = Ring.DefaultSegmentCount)
        {
            MeshProxy proxy = WireframeContainer.ProxyOf(container);
            Vector3 center = (localCornerA + localCornerB) * 0.5f;
            Vector2 size = RectangleShape.FlatSpan(localCornerA, localCornerB);
            return new RoundedRectangle(proxy, bone, center, Quaternion.identity, size, cornerRadius, segmentCount);
        }

        /// <summary>
        /// Creates a white rounded rectangle in world space, lying in the XZ plane of <paramref name="rotation"/>.
        /// </summary>
        /// <param name="segmentCount">
        /// Number of straight pieces a full circle of the corners is drawn with, a positive multiple of 4.
        /// </param>
        public static IRoundedRectangle CreateRoundedRectangle(
            this WireframeContainer container,
            Vector3 center,
            Quaternion rotation,
            Vector2 size,
            float cornerRadius,
            int segmentCount = Ring.DefaultSegmentCount)
        {
            MeshProxy proxy = WireframeContainer.ProxyOf(container);
            return new RoundedRectangle(proxy, null, center, rotation, size, cornerRadius, segmentCount);
        }
    }
}
