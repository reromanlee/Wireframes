using UnityEngine;

namespace reromanlee.Wireframes
{
    public sealed partial class LineContainer
    {
        public IBox CreateBox()
        {
            return new Box(Proxy, null, Vector3.zero, Quaternion.identity, Vector3.one * UnitSize);
        }

        public IBox CreateBox(Vector3 cornerA, Vector3 cornerB)
        {
            return new Box(Proxy, null, (cornerA + cornerB) * 0.5f, Quaternion.identity, cornerB - cornerA);
        }

        public IBox CreateBox(Transform bone, Vector3 localCornerA, Vector3 localCornerB)
        {
            Vector3 center = (localCornerA + localCornerB) * 0.5f;
            return new Box(Proxy, bone, center, Quaternion.identity, localCornerB - localCornerA);
        }

        public IBox CreateBox(Vector3 center, Quaternion rotation, Vector3 size)
        {
            return new Box(Proxy, null, center, rotation, size);
        }
    }
}
