using UnityEngine;

namespace reromanlee.Wireframes
{
    public sealed partial class LineContainer
    {
        public ICircle CreateCircle()
        {
            return new Circle(Proxy, null, Vector3.zero, Quaternion.identity, UnitRadius, Ring.DefaultSegments);
        }

        public ICircle CreateCircle(Vector3 center, float radius, int segments = Ring.DefaultSegments)
        {
            return new Circle(Proxy, null, center, Quaternion.identity, radius, segments);
        }

        public ICircle CreateCircle(Transform bone, Vector3 localCenter, float radius, int segments = Ring.DefaultSegments)
        {
            return new Circle(Proxy, bone, localCenter, Quaternion.identity, radius, segments);
        }

        public ICircle CreateCircle(Vector3 center, Quaternion rotation, float radius, int segments = Ring.DefaultSegments)
        {
            return new Circle(Proxy, null, center, rotation, radius, segments);
        }

        public ICircle CreateCircle(Vector3 center, Vector3 normal, float radius, int segments = Ring.DefaultSegments)
        {
            return new Circle(Proxy, null, center, ShapeRotations.FromNormal(normal), radius, segments);
        }
    }
}
