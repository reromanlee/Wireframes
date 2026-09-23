using UnityEngine;

namespace reromanlee.Wireframes
{
    public sealed partial class LineContainer
    {
        public IBox CreateBox()
        {
            return new Box(Proxy, Vector3.zero, Vector3.zero);
        }

        public IBox CreateBox(Vector3 cornerA, Vector3 cornerB)
        {
            return new Box(Proxy, cornerA, cornerB);
        }
    }
}
