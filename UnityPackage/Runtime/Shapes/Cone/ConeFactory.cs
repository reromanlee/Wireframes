using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>Creates cones in a <see cref="WireframeContainer"/>.</summary>
    public static class ConeFactory
    {
        /// <summary>Creates a white cone with its tip at the world origin and a base of radius 0.5, 1 unit along +Z.</summary>
        public static ICone CreateCone(this WireframeContainer container)
        {
            MeshProxy proxy = WireframeContainer.ProxyOf(container);
            return new Cone(
                proxy,
                null,
                Vector3.zero,
                Quaternion.identity,
                ShapeDefaults.Size,
                ShapeDefaults.Radius,
                Ring.DefaultSegmentCount);
        }

        /// <summary>
        /// Creates a white cone from a tip to the center of its base, both world positions. Its spin around the axis
        /// keeps the cone's +Y as close to world up as it can.
        /// </summary>
        /// <param name="segmentCount">Number of straight pieces the base ring is drawn with, a positive multiple of 4.</param>
        public static ICone CreateCone(
            this WireframeContainer container,
            Vector3 tip,
            Vector3 baseCenter,
            float radius,
            int segmentCount = Ring.DefaultSegmentCount)
        {
            MeshProxy proxy = WireframeContainer.ProxyOf(container);
            Quaternion rotation = ShapeRotations.FromAxis(baseCenter - tip, Vector3.up);
            return new Cone(proxy, null, tip, rotation, Vector3.Distance(tip, baseCenter), radius, segmentCount);
        }

        /// <summary>
        /// Creates a white cone that follows <paramref name="bone"/>, from a tip to the center of its base, both in the
        /// bone's local space. Its spin around the axis keeps the cone's +Y as close to the bone's up as it can.
        /// </summary>
        /// <param name="segmentCount">Number of straight pieces the base ring is drawn with, a positive multiple of 4.</param>
        public static ICone CreateCone(
            this WireframeContainer container,
            Transform bone,
            Vector3 localTip,
            Vector3 localBaseCenter,
            float radius,
            int segmentCount = Ring.DefaultSegmentCount)
        {
            MeshProxy proxy = WireframeContainer.ProxyOf(container);
            Quaternion rotation = ShapeRotations.FromAxis(localBaseCenter - localTip, Vector3.up);
            float length = Vector3.Distance(localTip, localBaseCenter);
            return new Cone(proxy, bone, localTip, rotation, length, radius, segmentCount);
        }

        /// <summary>
        /// Creates a white cone in world space with its tip at <paramref name="position"/> and its base
        /// <paramref name="length"/> along the +Z axis of <paramref name="rotation"/>.
        /// </summary>
        /// <param name="segmentCount">Number of straight pieces the base ring is drawn with, a positive multiple of 4.</param>
        public static ICone CreateCone(
            this WireframeContainer container,
            Vector3 position,
            Quaternion rotation,
            float length,
            float radius,
            int segmentCount = Ring.DefaultSegmentCount)
        {
            MeshProxy proxy = WireframeContainer.ProxyOf(container);
            return new Cone(proxy, null, position, rotation, length, radius, segmentCount);
        }
    }
}
