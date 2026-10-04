using UnityEngine;

namespace reromanlee.Wireframes.Editor
{
    /// <summary>Keeps glyph points in the glyph box, from 0 to 1 on both axes, and on its grid when snapping.</summary>
    internal static class GlyphGrid
    {
        internal static Vector2 Clamp(Vector2 point)
        {
            return new Vector2(Mathf.Clamp01(point.x), Mathf.Clamp01(point.y));
        }

        /// <summary>The grid point nearest to <paramref name="point"/>, for a grid of <paramref name="divisions"/> across the box.</summary>
        internal static Vector2 Snap(Vector2 point, int divisions)
        {
            float step = 1f / Mathf.Max(1, divisions);
            return Clamp(new Vector2(Mathf.Round(point.x / step) * step, Mathf.Round(point.y / step) * step));
        }
    }
}
