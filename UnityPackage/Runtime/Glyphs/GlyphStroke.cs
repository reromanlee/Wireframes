using System;
using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// One continuous line of a glyph: points in the glyph's box, from 0 to 1 on both axes, joined in order, and back to
    /// the first one when closed.
    /// </summary>
    [Serializable]
    internal struct GlyphStroke
    {
        internal const string PointsField = nameof(_points);
        internal const string IsClosedField = nameof(_isClosed);

        [SerializeField] private Vector2[] _points;
        [SerializeField] private bool _isClosed;

        internal GlyphStroke(Vector2[] points, bool isClosed)
        {
            _points = points;
            _isClosed = isClosed;
        }

        /// <summary>Points in the glyph's box, in drawing order; never null.</summary>
        internal Vector2[] Points
        {
            get => _points ?? Array.Empty<Vector2>();
        }

        /// <summary>True when an edge joins the last point back to the first.</summary>
        internal bool IsClosed
        {
            get => _isClosed;
        }

        /// <summary>
        /// True when the stroke draws anything: it has at least 2 points, or 3 when closed, and every point is a finite
        /// number. Others are skipped, as a polyline with too few points draws nothing.
        /// </summary>
        internal bool IsDrawn
        {
            get
            {
                Vector2[] points = Points;
                if (points.Length < (_isClosed ? 3 : 2))
                {
                    return false;
                }
                for (int i = 0; i < points.Length; i++)
                {
                    if (!float.IsFinite(points[i].x) || !float.IsFinite(points[i].y))
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        /// <summary>Number of edges the stroke draws once <see cref="IsDrawn"/>.</summary>
        internal int EdgeCount
        {
            get => _isClosed ? Points.Length : Points.Length - 1;
        }
    }
}
