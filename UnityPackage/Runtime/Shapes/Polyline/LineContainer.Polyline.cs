using System.Collections.Generic;
using UnityEngine;

namespace reromanlee.Wireframes
{
    public sealed partial class LineContainer
    {
        public IPolyline CreatePolyline(params Vector3[] points)
        {
            return new Polyline(Proxy, points, false);
        }

        public IPolyline CreatePolyline(IReadOnlyList<Vector3> points)
        {
            return new Polyline(Proxy, points, false);
        }

        public IPolyline CreatePolyline(params Transform[] bones)
        {
            return new Polyline(Proxy, bones, false);
        }

        public IPolyline CreatePolygon(params Vector3[] points)
        {
            return new Polyline(Proxy, points, true);
        }

        public IPolyline CreatePolygon(IReadOnlyList<Vector3> points)
        {
            return new Polyline(Proxy, points, true);
        }

        public IPolyline CreatePolygon(params Transform[] bones)
        {
            return new Polyline(Proxy, bones, true);
        }

        public IPolyline CreateTriangle(Vector3 pointA, Vector3 pointB, Vector3 pointC)
        {
            return new Polyline(Proxy, new[] { pointA, pointB, pointC }, true);
        }

        public IPolyline CreateTriangle(Transform boneA, Transform boneB, Transform boneC)
        {
            return new Polyline(Proxy, new[] { boneA, boneB, boneC }, true);
        }
    }
}
