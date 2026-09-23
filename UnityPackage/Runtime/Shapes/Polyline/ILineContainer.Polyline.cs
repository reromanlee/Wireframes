using System.Collections.Generic;
using UnityEngine;

namespace reromanlee.Wireframes
{
    public partial interface ILineContainer
    {
        /// <summary>Creates a white, open polyline through at least 2 world positions.</summary>
        IPolyline CreatePolyline(params Vector3[] points);

        /// <summary>Creates a white, open polyline through at least 2 world positions.</summary>
        IPolyline CreatePolyline(IReadOnlyList<Vector3> points);

        /// <summary>
        /// Creates a white, open polyline through the origins of at least 2 bones, with each point following its bone.
        /// A null bone puts that point at the world origin.
        /// </summary>
        IPolyline CreatePolyline(params Transform[] bones);

        /// <summary>Creates a white, closed polyline through at least 3 world positions.</summary>
        IPolyline CreatePolygon(params Vector3[] points);

        /// <summary>Creates a white, closed polyline through at least 3 world positions.</summary>
        IPolyline CreatePolygon(IReadOnlyList<Vector3> points);

        /// <summary>
        /// Creates a white, closed polyline through the origins of at least 3 bones, with each point following its
        /// bone. A null bone puts that point at the world origin.
        /// </summary>
        IPolyline CreatePolygon(params Transform[] bones);

        /// <summary>Creates a white triangle, a closed polyline through 3 world positions.</summary>
        IPolyline CreateTriangle(Vector3 pointA, Vector3 pointB, Vector3 pointC);

        /// <summary>
        /// Creates a white triangle through the origins of 3 bones, with each corner following its bone. A null bone
        /// puts that corner at the world origin.
        /// </summary>
        IPolyline CreateTriangle(Transform boneA, Transform boneB, Transform boneC);
    }
}
