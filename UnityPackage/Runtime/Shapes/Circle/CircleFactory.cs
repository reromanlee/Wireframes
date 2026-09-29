using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>Creates circles in a <see cref="WireframeContainer"/>.</summary>
    public static class CircleFactory
    {
        /// <summary>Creates a white circle of radius 0.5 around the world origin, lying flat in the XZ plane.</summary>
        public static ICircle CreateCircle(this WireframeContainer container)
        {
            MeshProxy proxy = WireframeContainer.ProxyOf(container);
            return new Circle(proxy, null, Vector3.zero, Quaternion.identity, ShapeDefaults.Radius, Ring.DefaultSegments);
        }

        /// <summary>Creates a white circle around a world position, lying flat in the world's XZ plane.</summary>
        /// <param name="segments">Number of straight pieces the circle is drawn with, at least 3.</param>
        public static ICircle CreateCircle(
            this WireframeContainer container, Vector3 center, float radius, int segments = Ring.DefaultSegments)
        {
            return new Circle(WireframeContainer.ProxyOf(container), null, center, Quaternion.identity, radius, segments);
        }

        /// <summary>
        /// Creates a white circle that follows <paramref name="bone"/>, around a position in the bone's local space
        /// and lying flat in the bone's XZ plane.
        /// </summary>
        /// <param name="segments">Number of straight pieces the circle is drawn with, at least 3.</param>
        public static ICircle CreateCircle(
            this WireframeContainer container,
            Transform bone,
            Vector3 localCenter,
            float radius,
            int segments = Ring.DefaultSegments)
        {
            MeshProxy proxy = WireframeContainer.ProxyOf(container);
            return new Circle(proxy, bone, localCenter, Quaternion.identity, radius, segments);
        }

        /// <summary>Creates a white circle in world space, lying in the XZ plane of <paramref name="rotation"/>.</summary>
        /// <param name="segments">Number of straight pieces the circle is drawn with, at least 3.</param>
        public static ICircle CreateCircle(
            this WireframeContainer container,
            Vector3 center,
            Quaternion rotation,
            float radius,
            int segments = Ring.DefaultSegments)
        {
            return new Circle(WireframeContainer.ProxyOf(container), null, center, rotation, radius, segments);
        }

        /// <summary>Creates a white circle around a world position, facing <paramref name="normal"/>.</summary>
        /// <param name="segments">Number of straight pieces the circle is drawn with, at least 3.</param>
        public static ICircle CreateCircle(
            this WireframeContainer container,
            Vector3 center,
            Vector3 normal,
            float radius,
            int segments = Ring.DefaultSegments)
        {
            MeshProxy proxy = WireframeContainer.ProxyOf(container);
            return new Circle(proxy, null, center, ShapeRotations.FromNormal(normal), radius, segments);
        }
    }
}
