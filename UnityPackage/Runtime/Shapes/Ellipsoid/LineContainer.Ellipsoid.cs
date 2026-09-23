using UnityEngine;

namespace reromanlee.Wireframes
{
    public sealed partial class LineContainer
    {
        public IEllipsoid CreateEllipsoid()
        {
            Vector3 radii = new(UnitRadius * 0.5f, UnitRadius * 0.5f, UnitRadius);
            return new Ellipsoid(Proxy, null, Vector3.zero, Quaternion.identity, radii, Ring.DefaultSegments);
        }

        public IEllipsoid CreateEllipsoid(Vector3 tipA, Vector3 tipB, float radius, int segments = Ring.DefaultSegments)
        {
            Quaternion rotation = ShapeRotations.FromAxis(tipB - tipA, Vector3.up);
            Vector3 radii = new(radius, radius, Vector3.Distance(tipA, tipB) * 0.5f);
            return new Ellipsoid(Proxy, null, (tipA + tipB) * 0.5f, rotation, radii, segments);
        }

        public IEllipsoid CreateEllipsoid(
            Transform bone, Vector3 localTipA, Vector3 localTipB, float radius, int segments = Ring.DefaultSegments)
        {
            Quaternion rotation = ShapeRotations.FromAxis(localTipB - localTipA, Vector3.up);
            Vector3 radii = new(radius, radius, Vector3.Distance(localTipA, localTipB) * 0.5f);
            return new Ellipsoid(Proxy, bone, (localTipA + localTipB) * 0.5f, rotation, radii, segments);
        }

        public IEllipsoid CreateEllipsoid(
            Vector3 center, Quaternion rotation, Vector3 radii, int segments = Ring.DefaultSegments)
        {
            return new Ellipsoid(Proxy, null, center, rotation, radii, segments);
        }
    }
}
