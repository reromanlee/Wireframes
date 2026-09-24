using UnityEngine;

namespace reromanlee.Wireframes
{
    public sealed partial class LineContainer
    {
        private const int UnitSpikeCount = 12;

        public ISpikedSphere CreateSpikedSphere()
        {
            float half = UnitRadius * 0.5f;
            return new SpikedSphere(Proxy, null, Vector3.zero, Quaternion.identity, half, half, UnitSpikeCount);
        }

        public ISpikedSphere CreateSpikedSphere(Vector3 center, float baseRadius, float spikeLength, int spikeCount)
        {
            return new SpikedSphere(Proxy, null, center, Quaternion.identity, baseRadius, spikeLength, spikeCount);
        }

        public ISpikedSphere CreateSpikedSphere(
            Transform bone, Vector3 localCenter, float baseRadius, float spikeLength, int spikeCount)
        {
            return new SpikedSphere(Proxy, bone, localCenter, Quaternion.identity, baseRadius, spikeLength, spikeCount);
        }

        public ISpikedSphere CreateSpikedSphere(
            Vector3 center, Quaternion rotation, float baseRadius, float spikeLength, int spikeCount)
        {
            return new SpikedSphere(Proxy, null, center, rotation, baseRadius, spikeLength, spikeCount);
        }
    }
}
