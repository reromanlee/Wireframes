using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>Creates capsules in a <see cref="WireframeContainer"/>.</summary>
    public static class CapsuleFactory
    {
        /// <summary>
        /// Creates a white capsule with spheres of radius 0.5 centered on the world origin and 1 unit along +Z.
        /// </summary>
        public static ICapsule CreateCapsule(this WireframeContainer container)
        {
            MeshProxy proxy = WireframeContainer.ProxyOf(container);
            return new Capsule(
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
        /// Creates a white capsule around two spheres of the same radius, centered on two world positions, like the
        /// capsule of <c>Physics.CapsuleCast</c>. Its spin around the axis keeps the capsule's +Y as close to world up
        /// as it can.
        /// </summary>
        /// <param name="segmentCount">Number of straight pieces each ring is drawn with, a positive multiple of 4.</param>
        public static ICapsule CreateCapsule(
            this WireframeContainer container,
            Vector3 centerA,
            Vector3 centerB,
            float radius,
            int segmentCount = Ring.DefaultSegmentCount)
        {
            return container.CreateCapsule(centerA, centerB, radius, radius, segmentCount);
        }

        /// <summary>
        /// Creates a white capsule around two spheres centered on two world positions, each with its own radius. Its
        /// spin around the axis keeps the capsule's +Y as close to world up as it can.
        /// </summary>
        /// <param name="segmentCount">Number of straight pieces each ring is drawn with, a positive multiple of 4.</param>
        public static ICapsule CreateCapsule(
            this WireframeContainer container,
            Vector3 centerA,
            Vector3 centerB,
            float radiusA,
            float radiusB,
            int segmentCount = Ring.DefaultSegmentCount)
        {
            MeshProxy proxy = WireframeContainer.ProxyOf(container);
            Quaternion rotation = ShapeRotations.FromAxis(centerB - centerA, Vector3.up);
            float length = Vector3.Distance(centerA, centerB);
            return new Capsule(proxy, null, centerA, rotation, length, radiusA, radiusB, segmentCount);
        }

        /// <summary>
        /// Creates a white capsule that follows <paramref name="bone"/>, around two spheres centered on positions in the
        /// bone's local space. Its spin around the axis keeps the capsule's +Y as close to the bone's up as it can.
        /// </summary>
        /// <param name="segmentCount">Number of straight pieces each ring is drawn with, a positive multiple of 4.</param>
        public static ICapsule CreateCapsule(
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
            return new Capsule(proxy, bone, localCenterA, rotation, length, radiusA, radiusB, segmentCount);
        }

        /// <summary>
        /// Creates a white capsule in world space whose first sphere is centered on <paramref name="position"/> and
        /// whose second sphere is centered <paramref name="length"/> along the +Z axis of <paramref name="rotation"/>.
        /// </summary>
        /// <param name="segmentCount">Number of straight pieces each ring is drawn with, a positive multiple of 4.</param>
        public static ICapsule CreateCapsule(
            this WireframeContainer container,
            Vector3 position,
            Quaternion rotation,
            float length,
            float radiusA,
            float radiusB,
            int segmentCount = Ring.DefaultSegmentCount)
        {
            MeshProxy proxy = WireframeContainer.ProxyOf(container);
            return new Capsule(proxy, null, position, rotation, length, radiusA, radiusB, segmentCount);
        }
    }
}
