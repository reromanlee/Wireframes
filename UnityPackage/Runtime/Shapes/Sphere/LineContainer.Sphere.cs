using UnityEngine;

namespace reromanlee.Wireframes
{
    public sealed partial class LineContainer
    {
        public ISphere CreateSphere()
        {
            return new Sphere(Proxy, null, Vector3.zero, Quaternion.identity, UnitRadius, Ring.DefaultSegments);
        }

        public ISphere CreateSphere(Vector3 center, float radius, int segments = Ring.DefaultSegments)
        {
            return new Sphere(Proxy, null, center, Quaternion.identity, radius, segments);
        }

        public ISphere CreateSphere(Transform bone, Vector3 localCenter, float radius, int segments = Ring.DefaultSegments)
        {
            return new Sphere(Proxy, bone, localCenter, Quaternion.identity, radius, segments);
        }

        public ISphere CreateSphere(Vector3 center, Quaternion rotation, float radius, int segments = Ring.DefaultSegments)
        {
            return new Sphere(Proxy, null, center, rotation, radius, segments);
        }
    }
}
