using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>Creates stadiums, the flat outlines of capsules, in a <see cref="WireframeContainer"/>.</summary>
    public static class StadiumFactory
    {
        /// <summary>
        /// Creates a white stadium lying flat in the XZ plane, around circles of radius 0.5 centered on the world origin
        /// and 1 unit along +Z.
        /// </summary>
        public static IStadium CreateStadium(this WireframeContainer container)
        {
            MeshProxy proxy = WireframeContainer.ProxyOf(container);
            return new Stadium(
                proxy,
                null,
                Vector3.zero,
                Quaternion.identity,
                ShapeDefaults.Size,
                ShapeDefaults.Radius,
                ShapeDefaults.Radius,
                Ring.DefaultSegmentCount);
        }

        /// <summary>
        /// Creates a white stadium around two circles of the same radius, centered on two world positions. It lies as
        /// flat as it can: its normal stays as close to world up as the centers allow.
        /// </summary>
        /// <param name="segmentCount">Number of straight pieces a full circle would be drawn with, a positive multiple of 4.</param>
        public static IStadium CreateStadium(
            this WireframeContainer container,
            Vector3 centerA,
            Vector3 centerB,
            float radius,
            int segmentCount = Ring.DefaultSegmentCount)
        {
            return container.CreateStadium(centerA, centerB, radius, radius, Vector3.up, segmentCount);
        }

        /// <summary>
        /// Creates a white stadium around two circles centered on two world positions, each with its own radius. It lies
        /// as flat as it can: its normal stays as close to world up as the centers allow.
        /// </summary>
        /// <param name="segmentCount">Number of straight pieces a full circle would be drawn with, a positive multiple of 4.</param>
        public static IStadium CreateStadium(
            this WireframeContainer container,
            Vector3 centerA,
            Vector3 centerB,
            float radiusA,
            float radiusB,
            int segmentCount = Ring.DefaultSegmentCount)
        {
            return container.CreateStadium(centerA, centerB, radiusA, radiusB, Vector3.up, segmentCount);
        }

        /// <summary>
        /// Creates a white stadium around two circles centered on two world positions, facing as close to
        /// <paramref name="normal"/> as the centers allow.
        /// </summary>
        /// <param name="segmentCount">Number of straight pieces a full circle would be drawn with, a positive multiple of 4.</param>
        public static IStadium CreateStadium(
            this WireframeContainer container,
            Vector3 centerA,
            Vector3 centerB,
            float radiusA,
            float radiusB,
            Vector3 normal,
            int segmentCount = Ring.DefaultSegmentCount)
        {
            MeshProxy proxy = WireframeContainer.ProxyOf(container);
            Quaternion rotation = ShapeRotations.FromAxis(centerB - centerA, normal);
            float length = Vector3.Distance(centerA, centerB);
            return new Stadium(proxy, null, centerA, rotation, length, radiusA, radiusB, segmentCount);
        }

        /// <summary>
        /// Creates a white stadium that follows <paramref name="bone"/>, around two circles centered on positions in the
        /// bone's local space. It lies as flat in the bone's XZ plane as the centers allow.
        /// </summary>
        /// <param name="segmentCount">Number of straight pieces a full circle would be drawn with, a positive multiple of 4.</param>
        public static IStadium CreateStadium(
            this WireframeContainer container,
            Transform bone,
            Vector3 localCenterA,
            Vector3 localCenterB,
            float radiusA,
            float radiusB,
            int segmentCount = Ring.DefaultSegmentCount)
        {
            MeshProxy proxy = WireframeContainer.ProxyOf(container);
            Quaternion rotation = ShapeRotations.FromAxis(localCenterB - localCenterA, Vector3.up);
            float length = Vector3.Distance(localCenterA, localCenterB);
            return new Stadium(proxy, bone, localCenterA, rotation, length, radiusA, radiusB, segmentCount);
        }

        /// <summary>
        /// Creates a white stadium in world space, lying in the XZ plane of <paramref name="rotation"/>, whose first
        /// circle is centered on <paramref name="position"/> and whose second is centered <paramref name="length"/>
        /// along the rotation's +Z axis.
        /// </summary>
        /// <param name="segmentCount">Number of straight pieces a full circle would be drawn with, a positive multiple of 4.</param>
        public static IStadium CreateStadium(
            this WireframeContainer container,
            Vector3 position,
            Quaternion rotation,
            float length,
            float radiusA,
            float radiusB,
            int segmentCount = Ring.DefaultSegmentCount)
        {
            MeshProxy proxy = WireframeContainer.ProxyOf(container);
            return new Stadium(proxy, null, position, rotation, length, radiusA, radiusB, segmentCount);
        }
    }
}
