using System.Collections.Generic;
using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>Creates polylines, polygons and triangles in a <see cref="WireframeContainer"/>.</summary>
    public static class PolylineFactory
    {
        /// <summary>Creates a white, open polyline through at least 2 world positions.</summary>
        public static IPolyline CreatePolyline(this WireframeContainer container, params Vector3[] points)
        {
            return new Polyline(WireframeContainer.ProxyOf(container), points, false);
        }

        /// <summary>Creates a white, open polyline through at least 2 world positions.</summary>
        public static IPolyline CreatePolyline(this WireframeContainer container, IReadOnlyList<Vector3> points)
        {
            return new Polyline(WireframeContainer.ProxyOf(container), points, false);
        }

        /// <summary>
        /// Creates a white, open polyline through the origins of at least 2 bones, with each point following its bone.
        /// A null bone puts that point at the world origin.
        /// </summary>
        public static IPolyline CreatePolyline(this WireframeContainer container, params Transform[] bones)
        {
            return new Polyline(WireframeContainer.ProxyOf(container), bones, false);
        }

        /// <summary>Creates a white, closed polyline through at least 3 world positions.</summary>
        public static IPolyline CreatePolygon(this WireframeContainer container, params Vector3[] points)
        {
            return new Polyline(WireframeContainer.ProxyOf(container), points, true);
        }

        /// <summary>Creates a white, closed polyline through at least 3 world positions.</summary>
        public static IPolyline CreatePolygon(this WireframeContainer container, IReadOnlyList<Vector3> points)
        {
            return new Polyline(WireframeContainer.ProxyOf(container), points, true);
        }

        /// <summary>
        /// Creates a white, closed polyline through the origins of at least 3 bones, with each point following its
        /// bone. A null bone puts that point at the world origin.
        /// </summary>
        public static IPolyline CreatePolygon(this WireframeContainer container, params Transform[] bones)
        {
            return new Polyline(WireframeContainer.ProxyOf(container), bones, true);
        }

        /// <summary>Creates a white triangle, a closed polyline through 3 world positions.</summary>
        public static IPolyline CreateTriangle(
            this WireframeContainer container, Vector3 pointA, Vector3 pointB, Vector3 pointC)
        {
            return new Polyline(WireframeContainer.ProxyOf(container), new[] { pointA, pointB, pointC }, true);
        }

        /// <summary>
        /// Creates a white triangle through the origins of 3 bones, with each corner following its bone. A null bone
        /// puts that corner at the world origin.
        /// </summary>
        public static IPolyline CreateTriangle(
            this WireframeContainer container, Transform boneA, Transform boneB, Transform boneC)
        {
            return new Polyline(WireframeContainer.ProxyOf(container), new[] { boneA, boneB, boneC }, true);
        }
    }
}
