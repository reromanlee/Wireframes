using UnityEngine;

namespace reromanlee.Wireframes
{
    public sealed partial class LineContainer
    {
        private const float UnitCornerRadius = 0.25f;

        public IRoundedRectangle CreateRoundedRectangle()
        {
            return new RoundedRectangle(
                Proxy, null, Vector3.zero, Quaternion.identity, Vector2.one * UnitSize, UnitCornerRadius, Ring.DefaultSegments);
        }

        public IRoundedRectangle CreateRoundedRectangle(
            Vector3 cornerA, Vector3 cornerB, float cornerRadius, int segments = Ring.DefaultSegments)
        {
            Vector3 center = (cornerA + cornerB) * 0.5f;
            Vector2 size = FlatSpan(cornerA, cornerB);
            return new RoundedRectangle(Proxy, null, center, Quaternion.identity, size, cornerRadius, segments);
        }

        public IRoundedRectangle CreateRoundedRectangle(
            Transform bone,
            Vector3 localCornerA,
            Vector3 localCornerB,
            float cornerRadius,
            int segments = Ring.DefaultSegments)
        {
            Vector3 center = (localCornerA + localCornerB) * 0.5f;
            Vector2 size = FlatSpan(localCornerA, localCornerB);
            return new RoundedRectangle(Proxy, bone, center, Quaternion.identity, size, cornerRadius, segments);
        }

        public IRoundedRectangle CreateRoundedRectangle(
            Vector3 center, Quaternion rotation, Vector2 size, float cornerRadius, int segments = Ring.DefaultSegments)
        {
            return new RoundedRectangle(Proxy, null, center, rotation, size, cornerRadius, segments);
        }
    }
}
