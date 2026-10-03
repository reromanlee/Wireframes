using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>Creates spiked spheres in a <see cref="WireframeContainer"/>.</summary>
    public static class SpikedSphereFactory
    {
        internal const int DefaultSpikeCount = 12;

        /// <summary>
        /// Creates a white spiked sphere around the world origin: a dodecahedron with corners 0.25 from the center and
        /// 12 spikes reaching 0.5.
        /// </summary>
        public static ISpikedSphere CreateSpikedSphere(this WireframeContainer container)
        {
            MeshProxy proxy = WireframeContainer.ProxyOf(container);
            float half = ShapeDefaults.Radius * 0.5f;
            return proxy.Add(new SpikedSphere(null, Vector3.zero, Quaternion.identity, half, half, DefaultSpikeCount));
        }

        /// <summary>Creates a white, world-aligned spiked sphere around a world position.</summary>
        /// <param name="spikeCount">4, 6, 8, 12 or 20: the number of faces of the Platonic solid it is built on.</param>
        public static ISpikedSphere CreateSpikedSphere(
            this WireframeContainer container, Vector3 center, float baseRadius, float spikeLength, int spikeCount)
        {
            MeshProxy proxy = WireframeContainer.ProxyOf(container);
            return proxy.Add(new SpikedSphere(null, center, Quaternion.identity, baseRadius, spikeLength, spikeCount));
        }

        /// <summary>
        /// Creates a white spiked sphere that follows <paramref name="bone"/>, around a position in the bone's local
        /// space and aligned to the bone's axes.
        /// </summary>
        /// <param name="spikeCount">4, 6, 8, 12 or 20: the number of faces of the Platonic solid it is built on.</param>
        public static ISpikedSphere CreateSpikedSphere(
            this WireframeContainer container,
            Transform bone,
            Vector3 localCenter,
            float baseRadius,
            float spikeLength,
            int spikeCount)
        {
            MeshProxy proxy = WireframeContainer.ProxyOf(container);
            return proxy.Add(new SpikedSphere(
                bone, localCenter, Quaternion.identity, baseRadius, spikeLength, spikeCount));
        }

        /// <summary>Creates a white spiked sphere in world space, turned by <paramref name="rotation"/>.</summary>
        /// <param name="spikeCount">4, 6, 8, 12 or 20: the number of faces of the Platonic solid it is built on.</param>
        public static ISpikedSphere CreateSpikedSphere(
            this WireframeContainer container,
            Vector3 center,
            Quaternion rotation,
            float baseRadius,
            float spikeLength,
            int spikeCount)
        {
            MeshProxy proxy = WireframeContainer.ProxyOf(container);
            return proxy.Add(new SpikedSphere(null, center, rotation, baseRadius, spikeLength, spikeCount));
        }
    }
}
