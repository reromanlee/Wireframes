using UnityEngine;

namespace reromanlee.Wireframes
{
    public sealed partial class LineContainer
    {
        public IStadium CreateStadium()
        {
            return new Stadium(
                Proxy, null, Vector3.zero, Quaternion.identity, UnitSize, UnitRadius, UnitRadius, Ring.DefaultSegments);
        }

        public IStadium CreateStadium(Vector3 centerA, Vector3 centerB, float radius, int segments = Ring.DefaultSegments)
        {
            return CreateStadium(centerA, centerB, radius, radius, Vector3.up, segments);
        }

        public IStadium CreateStadium(
            Vector3 centerA, Vector3 centerB, float radiusA, float radiusB, int segments = Ring.DefaultSegments)
        {
            return CreateStadium(centerA, centerB, radiusA, radiusB, Vector3.up, segments);
        }

        public IStadium CreateStadium(
            Vector3 centerA,
            Vector3 centerB,
            float radiusA,
            float radiusB,
            Vector3 normal,
            int segments = Ring.DefaultSegments)
        {
            Quaternion rotation = ShapeRotations.FromAxis(centerB - centerA, normal);
            float length = Vector3.Distance(centerA, centerB);
            return new Stadium(Proxy, null, centerA, rotation, length, radiusA, radiusB, segments);
        }

        public IStadium CreateStadium(
            Transform bone,
            Vector3 localCenterA,
            Vector3 localCenterB,
            float radiusA,
            float radiusB,
            int segments = Ring.DefaultSegments)
        {
            Quaternion rotation = ShapeRotations.FromAxis(localCenterB - localCenterA, Vector3.up);
            float length = Vector3.Distance(localCenterA, localCenterB);
            return new Stadium(Proxy, bone, localCenterA, rotation, length, radiusA, radiusB, segments);
        }

        public IStadium CreateStadium(
            Vector3 position,
            Quaternion rotation,
            float length,
            float radiusA,
            float radiusB,
            int segments = Ring.DefaultSegments)
        {
            return new Stadium(Proxy, null, position, rotation, length, radiusA, radiusB, segments);
        }
    }
}
