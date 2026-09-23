using UnityEngine;

namespace reromanlee.Wireframes
{
    public partial interface ILineContainer
    {
        /// <summary>
        /// Creates a white spiked sphere around the world origin: a dodecahedron with corners 0.25 from the center and
        /// 12 spikes reaching 0.5.
        /// </summary>
        ISpikedSphere CreateSpikedSphere();

        /// <summary>Creates a white, world-aligned spiked sphere around a world position.</summary>
        /// <param name="spikeCount">4, 6, 8, 12 or 20: the number of faces of the Platonic solid it is built on.</param>
        ISpikedSphere CreateSpikedSphere(Vector3 center, float baseRadius, float spikeLength, int spikeCount);

        /// <summary>
        /// Creates a white spiked sphere that follows <paramref name="bone"/>, around a position in the bone's local
        /// space and aligned to the bone's axes.
        /// </summary>
        /// <param name="spikeCount">4, 6, 8, 12 or 20: the number of faces of the Platonic solid it is built on.</param>
        ISpikedSphere CreateSpikedSphere(
            Transform bone, Vector3 localCenter, float baseRadius, float spikeLength, int spikeCount);

        /// <summary>Creates a white spiked sphere in world space, turned by <paramref name="rotation"/>.</summary>
        /// <param name="spikeCount">4, 6, 8, 12 or 20: the number of faces of the Platonic solid it is built on.</param>
        ISpikedSphere CreateSpikedSphere(
            Vector3 center, Quaternion rotation, float baseRadius, float spikeLength, int spikeCount);
    }
}
