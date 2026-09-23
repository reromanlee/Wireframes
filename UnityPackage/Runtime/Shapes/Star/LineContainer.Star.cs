using UnityEngine;

namespace reromanlee.Wireframes
{
    public sealed partial class LineContainer
    {
        private const float UnitStarInnerRadius = 0.2f;
        private const int UnitStarPoints = 5;

        public IStar CreateStar()
        {
            return new Star(
                Proxy, null, Vector3.zero, Quaternion.identity, UnitStarInnerRadius, UnitRadius, UnitStarPoints);
        }

        public IStar CreateStar(Vector3 center, float innerRadius, float outerRadius, int points)
        {
            return new Star(Proxy, null, center, Quaternion.identity, innerRadius, outerRadius, points);
        }

        public IStar CreateStar(Transform bone, Vector3 localCenter, float innerRadius, float outerRadius, int points)
        {
            return new Star(Proxy, bone, localCenter, Quaternion.identity, innerRadius, outerRadius, points);
        }

        public IStar CreateStar(Vector3 center, Quaternion rotation, float innerRadius, float outerRadius, int points)
        {
            return new Star(Proxy, null, center, rotation, innerRadius, outerRadius, points);
        }
    }
}
