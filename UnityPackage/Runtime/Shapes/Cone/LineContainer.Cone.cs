using UnityEngine;

namespace reromanlee.Wireframes
{
    public sealed partial class LineContainer
    {
        public ICone CreateCone()
        {
            return new Cone(Proxy, null, Vector3.zero, Quaternion.identity, UnitSize, UnitRadius, Ring.DefaultSegments);
        }

        public ICone CreateCone(Vector3 tip, Vector3 baseCenter, float radius, int segments = Ring.DefaultSegments)
        {
            Quaternion rotation = ShapeRotations.FromAxis(baseCenter - tip, Vector3.up);
            return new Cone(Proxy, null, tip, rotation, Vector3.Distance(tip, baseCenter), radius, segments);
        }

        public ICone CreateCone(
            Transform bone, Vector3 localTip, Vector3 localBaseCenter, float radius, int segments = Ring.DefaultSegments)
        {
            Quaternion rotation = ShapeRotations.FromAxis(localBaseCenter - localTip, Vector3.up);
            float length = Vector3.Distance(localTip, localBaseCenter);
            return new Cone(Proxy, bone, localTip, rotation, length, radius, segments);
        }

        public ICone CreateCone(
            Vector3 position, Quaternion rotation, float length, float radius, int segments = Ring.DefaultSegments)
        {
            return new Cone(Proxy, null, position, rotation, length, radius, segments);
        }
    }
}
