using UnityEngine;

namespace reromanlee.Wireframes
{
    public sealed partial class LineContainer
    {
        public ICylinder CreateCylinder()
        {
            return new Cylinder(Proxy, null, Vector3.zero, Quaternion.identity, UnitSize, UnitRadius, Ring.DefaultSegments);
        }

        public ICylinder CreateCylinder(Vector3 endA, Vector3 endB, float radius, int segments = Ring.DefaultSegments)
        {
            Quaternion rotation = ShapeRotations.FromAxis(endB - endA, Vector3.up);
            return new Cylinder(Proxy, null, endA, rotation, Vector3.Distance(endA, endB), radius, segments);
        }

        public ICylinder CreateCylinder(
            Transform bone, Vector3 localEndA, Vector3 localEndB, float radius, int segments = Ring.DefaultSegments)
        {
            Quaternion rotation = ShapeRotations.FromAxis(localEndB - localEndA, Vector3.up);
            return new Cylinder(Proxy, bone, localEndA, rotation, Vector3.Distance(localEndA, localEndB), radius, segments);
        }

        public ICylinder CreateCylinder(
            Vector3 position, Quaternion rotation, float length, float radius, int segments = Ring.DefaultSegments)
        {
            return new Cylinder(Proxy, null, position, rotation, length, radius, segments);
        }
    }
}
