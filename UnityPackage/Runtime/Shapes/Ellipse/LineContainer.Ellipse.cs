using UnityEngine;

namespace reromanlee.Wireframes
{
    public sealed partial class LineContainer
    {
        public IEllipse CreateEllipse()
        {
            Vector2 radii = new(UnitRadius * 0.5f, UnitRadius);
            return new Ellipse(Proxy, null, Vector3.zero, Quaternion.identity, radii, Ring.DefaultSegments);
        }

        public IEllipse CreateEllipse(Vector3 tipA, Vector3 tipB, float radius, int segments = Ring.DefaultSegments)
        {
            return CreateEllipse(tipA, tipB, radius, Vector3.up, segments);
        }

        public IEllipse CreateEllipse(
            Vector3 tipA, Vector3 tipB, float radius, Vector3 normal, int segments = Ring.DefaultSegments)
        {
            Quaternion rotation = ShapeRotations.FromAxis(tipB - tipA, normal);
            Vector2 radii = new(radius, Vector3.Distance(tipA, tipB) * 0.5f);
            return new Ellipse(Proxy, null, (tipA + tipB) * 0.5f, rotation, radii, segments);
        }

        public IEllipse CreateEllipse(
            Transform bone, Vector3 localTipA, Vector3 localTipB, float radius, int segments = Ring.DefaultSegments)
        {
            Quaternion rotation = ShapeRotations.FromAxis(localTipB - localTipA, Vector3.up);
            Vector2 radii = new(radius, Vector3.Distance(localTipA, localTipB) * 0.5f);
            return new Ellipse(Proxy, bone, (localTipA + localTipB) * 0.5f, rotation, radii, segments);
        }

        public IEllipse CreateEllipse(Vector3 center, Quaternion rotation, Vector2 radii, int segments = Ring.DefaultSegments)
        {
            return new Ellipse(Proxy, null, center, rotation, radii, segments);
        }
    }
}
