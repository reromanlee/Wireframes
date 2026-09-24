using UnityEngine;

namespace reromanlee.Wireframes
{
    public sealed partial class LineContainer
    {
        public IRectangle CreateRectangle()
        {
            return new Rectangle(Proxy, null, Vector3.zero, Quaternion.identity, Vector2.one * UnitSize);
        }

        public IRectangle CreateRectangle(Vector3 cornerA, Vector3 cornerB)
        {
            Vector3 center = (cornerA + cornerB) * 0.5f;
            return new Rectangle(Proxy, null, center, Quaternion.identity, FlatSpan(cornerA, cornerB));
        }

        public IRectangle CreateRectangle(Transform bone, Vector3 localCornerA, Vector3 localCornerB)
        {
            Vector3 center = (localCornerA + localCornerB) * 0.5f;
            return new Rectangle(Proxy, bone, center, Quaternion.identity, FlatSpan(localCornerA, localCornerB));
        }

        public IRectangle CreateRectangle(Vector3 center, Quaternion rotation, Vector2 size)
        {
            return new Rectangle(Proxy, null, center, rotation, size);
        }

        /// <summary>Size of the XZ rectangle spanned by two corners; their difference in height is ignored.</summary>
        private static Vector2 FlatSpan(Vector3 cornerA, Vector3 cornerB)
        {
            return new Vector2(cornerB.x - cornerA.x, cornerB.z - cornerA.z);
        }
    }
}
