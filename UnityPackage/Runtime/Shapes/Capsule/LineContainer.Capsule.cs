using UnityEngine;

namespace reromanlee.Wireframes
{
    public sealed partial class LineContainer
    {
        public ICapsule CreateCapsule()
        {
            return new Capsule(
                Proxy, null, Vector3.zero, Quaternion.identity, UnitSize, UnitRadius, UnitRadius, Ring.DefaultSegments);
        }

        public ICapsule CreateCapsule(Vector3 centerA, Vector3 centerB, float radius, int segments = Ring.DefaultSegments)
        {
            return CreateCapsule(centerA, centerB, radius, radius, segments);
        }

        public ICapsule CreateCapsule(
            Vector3 centerA, Vector3 centerB, float radiusA, float radiusB, int segments = Ring.DefaultSegments)
        {
            Quaternion rotation = ShapeRotations.FromAxis(centerB - centerA, Vector3.up);
            float length = Vector3.Distance(centerA, centerB);
            return new Capsule(Proxy, null, centerA, rotation, length, radiusA, radiusB, segments);
        }

        public ICapsule CreateCapsule(
            Transform bone,
            Vector3 localCenterA,
            Vector3 localCenterB,
            float radiusA,
            float radiusB,
            int segments = Ring.DefaultSegments)
        {
            Quaternion rotation = ShapeRotations.FromAxis(localCenterB - localCenterA, Vector3.up);
            float length = Vector3.Distance(localCenterA, localCenterB);
            return new Capsule(Proxy, bone, localCenterA, rotation, length, radiusA, radiusB, segments);
        }

        public ICapsule CreateCapsule(
            Vector3 position,
            Quaternion rotation,
            float length,
            float radiusA,
            float radiusB,
            int segments = Ring.DefaultSegments)
        {
            return new Capsule(Proxy, null, position, rotation, length, radiusA, radiusB, segments);
        }
    }
}
