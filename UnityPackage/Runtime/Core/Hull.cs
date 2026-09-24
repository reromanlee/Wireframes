using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// The outline that wraps two spheres, or two circles, whose centers lie on the +Z axis: the way capsules and
    /// stadiums are drawn.
    /// </summary>
    internal static class Hull
    {
        /// <summary>
        /// Where the outline touches the spheres. Each touching point sits <paramref name="sine"/> times its sphere's
        /// radius back along the axis from the center and <paramref name="cosine"/> times the radius out from it, so
        /// equal radii give 0 and 1. When one sphere holds the other, the cosine is 0.
        /// </summary>
        internal static void Tangent(float radiusA, float radiusB, float length, out float sine, out float cosine)
        {
            float difference = radiusB - radiusA;
            if (length > 0f)
            {
                sine = Mathf.Clamp(difference / length, -1f, 1f);
            }
            else
            {
                sine = difference > 0f ? 1f : difference < 0f ? -1f : 0f;
            }
            cosine = Mathf.Sqrt(1f - sine * sine);
        }

        /// <summary>
        /// A point on an arc over a sphere's pole. Angle 0 is the pole, and growing angles tilt toward
        /// <paramref name="side"/>.
        /// </summary>
        internal static Vector3 ArcPoint(Vector3 center, float radius, Vector3 side, Vector3 pole, float angle)
        {
            return center + (side * Mathf.Sin(angle) + pole * Mathf.Cos(angle)) * radius;
        }
    }
}
