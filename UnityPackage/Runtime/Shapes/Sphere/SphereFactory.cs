using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>Creates spheres in a <see cref="WireframeContainer"/>.</summary>
    public static class SphereFactory
    {
        /// <summary>Creates a white sphere of radius 0.5 around the world origin.</summary>
        public static ISphere CreateSphere(this WireframeContainer container)
        {
            MeshProxy proxy = WireframeContainer.ProxyOf(container);
            return new Sphere(proxy, null, Vector3.zero, Quaternion.identity, ShapeDefaults.Radius, Ring.DefaultSegments);
        }

        /// <summary>Creates a white, world-aligned sphere around a world position.</summary>
        /// <param name="segments">Number of straight pieces each great circle is drawn with, at least 3.</param>
        public static ISphere CreateSphere(
            this WireframeContainer container, Vector3 center, float radius, int segments = Ring.DefaultSegments)
        {
            return new Sphere(WireframeContainer.ProxyOf(container), null, center, Quaternion.identity, radius, segments);
        }

        /// <summary>
        /// Creates a white sphere that follows <paramref name="bone"/>, around a position in the bone's local space and
        /// aligned to the bone's axes.
        /// </summary>
        /// <param name="segments">Number of straight pieces each great circle is drawn with, at least 3.</param>
        public static ISphere CreateSphere(
            this WireframeContainer container,
            Transform bone,
            Vector3 localCenter,
            float radius,
            int segments = Ring.DefaultSegments)
        {
            MeshProxy proxy = WireframeContainer.ProxyOf(container);
            return new Sphere(proxy, bone, localCenter, Quaternion.identity, radius, segments);
        }

        /// <summary>Creates a white sphere in world space, with its great circles turned by <paramref name="rotation"/>.</summary>
        /// <param name="segments">Number of straight pieces each great circle is drawn with, at least 3.</param>
        public static ISphere CreateSphere(
            this WireframeContainer container,
            Vector3 center,
            Quaternion rotation,
            float radius,
            int segments = Ring.DefaultSegments)
        {
            return new Sphere(WireframeContainer.ProxyOf(container), null, center, rotation, radius, segments);
        }
    }
}
