using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>Creates frustums, prisms and regular pyramids in a <see cref="WireframeContainer"/>.</summary>
    public static class FrustumFactory
    {
        internal const int DefaultSideCount = 4;

        /// <summary>
        /// Creates a white square frustum from the world origin, 1 unit along +Z, with corners 0.25 out at end A and
        /// 0.5 out at end B.
        /// </summary>
        public static IFrustum CreateFrustum(this WireframeContainer container)
        {
            MeshProxy proxy = WireframeContainer.ProxyOf(container);
            return proxy.Add(new Frustum(
                null,
                Vector3.zero,
                Quaternion.identity,
                ShapeDefaults.Size,
                ShapeDefaults.Radius * 0.5f,
                ShapeDefaults.Radius,
                DefaultSideCount));
        }

        /// <summary>
        /// Creates a white frustum between the centers of its two polygons, both world positions. Its spin around the
        /// axis keeps the frustum's +Y as close to world up as it can.
        /// </summary>
        /// <param name="sideCount">Number of sides of each polygon, from 3 to 1024.</param>
        public static IFrustum CreateFrustum(
            this WireframeContainer container, Vector3 endA, Vector3 endB, float radiusA, float radiusB, int sideCount)
        {
            MeshProxy proxy = WireframeContainer.ProxyOf(container);
            Quaternion rotation = ShapeRotations.FromAxis(endB - endA, Vector3.up);
            return proxy.Add(new Frustum(
                null, endA, rotation, Vector3.Distance(endA, endB), radiusA, radiusB, sideCount));
        }

        /// <summary>
        /// Creates a white frustum that follows <paramref name="bone"/>, between the centers of its two polygons in the
        /// bone's local space. Its spin around the axis keeps the frustum's +Y as close to the bone's up as it can.
        /// </summary>
        /// <param name="sideCount">Number of sides of each polygon, from 3 to 1024.</param>
        public static IFrustum CreateFrustum(
            this WireframeContainer container,
            Transform bone,
            Vector3 localEndA,
            Vector3 localEndB,
            float radiusA,
            float radiusB,
            int sideCount)
        {
            MeshProxy proxy = WireframeContainer.ProxyOf(container);
            Quaternion rotation = ShapeRotations.FromAxis(localEndB - localEndA, Vector3.up);
            float length = Vector3.Distance(localEndA, localEndB);
            return proxy.Add(new Frustum(bone, localEndA, rotation, length, radiusA, radiusB, sideCount));
        }

        /// <summary>
        /// Creates a white frustum in world space that starts at <paramref name="position"/> and runs
        /// <paramref name="length"/> along the +Z axis of <paramref name="rotation"/>.
        /// </summary>
        /// <param name="sideCount">Number of sides of each polygon, from 3 to 1024.</param>
        public static IFrustum CreateFrustum(
            this WireframeContainer container,
            Vector3 position,
            Quaternion rotation,
            float length,
            float radiusA,
            float radiusB,
            int sideCount)
        {
            MeshProxy proxy = WireframeContainer.ProxyOf(container);
            return proxy.Add(new Frustum(null, position, rotation, length, radiusA, radiusB, sideCount));
        }
    }
}
