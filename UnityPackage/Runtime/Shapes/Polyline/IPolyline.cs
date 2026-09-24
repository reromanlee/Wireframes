using System;
using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// Points joined in order by edges, with a last edge back to the first point when <see cref="IsClosed"/>. Each
    /// point can follow its own bone, like a line's endpoints. The number of points and whether the polyline is closed
    /// are fixed at creation. Methods that take a point index throw <see cref="ArgumentOutOfRangeException"/> for an
    /// index outside <c>0</c> to <see cref="PointCount"/> - 1.
    /// </summary>
    public interface IPolyline : IShape
    {
        /// <summary>Number of points.</summary>
        int PointCount { get; }

        /// <summary>True when an edge joins the last point back to the first, as in a polygon.</summary>
        bool IsClosed { get; }

        /// <summary>Returns a point relative to its bone, or in world space without a bone.</summary>
        Vector3 GetLocalPosition(int index);

        /// <summary>Sets a point relative to its bone, or in world space without a bone.</summary>
        void SetLocalPosition(int index, Vector3 position);

        /// <summary>Returns a point in world space, converted through its bone's current pose.</summary>
        Vector3 GetWorldPosition(int index);

        /// <summary>Sets a point in world space, converted through its bone's current pose.</summary>
        void SetWorldPosition(int index, Vector3 position);

        /// <summary>Returns a point's color. Colors are interpolated along the edges.</summary>
        Color GetColor(int index);

        /// <summary>Sets a point's color. Colors are interpolated along the edges.</summary>
        void SetColor(int index, Color color);

        /// <summary>Returns the Transform a point follows, or null for world space.</summary>
        Transform GetBone(int index);

        /// <summary>
        /// Sets the Transform a point follows, or null for world space. Changing it keeps the point's world position,
        /// like reparenting a Transform. If the bone is destroyed, the point stays where it was and becomes world space.
        /// </summary>
        void SetBone(int index, Transform bone);
    }
}
