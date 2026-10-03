using System;
using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// Draws a flat ellipse on its GameObject, centered on <see cref="WireframeCenteredShape.Center"/> in the XZ plane of
    /// its rotation.
    /// </summary>
    [AddComponentMenu("Wireframes/Ellipse")]
    [ExecuteAlways]
    [HelpURL(HelpUrl)]
    public sealed class WireframeEllipse : WireframeCenteredShape
    {
        [Tooltip("Radius along the ellipse's X axis and along its Z axis, in the GameObject's local units.")]
        [SerializeField] private Vector2 _radii = new(ShapeDefaults.Radius * 0.5f, ShapeDefaults.Radius);

        [Tooltip("Number of straight pieces the ellipse is drawn with, from 3 to 1024.")]
        [SerializeField, Delayed] private int _segmentCount = Ring.DefaultSegmentCount;

        /// <summary>
        /// Semi-axes in the GameObject's local units: x is the radius along the ellipse's X axis and y along its Z axis.
        /// 0.25 by 0.5 by default.
        /// </summary>
        public Vector2 Radii
        {
            get => _radii;
            set
            {
                _radii = value;
                if (TryGetShape(out Ellipse ellipse))
                {
                    ellipse.Radii = value;
                }
            }
        }

        /// <summary>
        /// Number of straight pieces the ellipse is drawn with, from 3 to 1024. 32 by default. Changing it creates the
        /// shape again.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The count is outside 3 to 1024.</exception>
        public int SegmentCount
        {
            get => _segmentCount;
            set
            {
                _segmentCount = ShapeCounts.CheckSegmentCount(value);
                Refresh();
            }
        }

        internal override int BuildSignature
        {
            get => _segmentCount;
        }

        internal override Shape CreateShape(Transform bone)
        {
            return new Ellipse(bone, Center, Rotation, _radii, _segmentCount);
        }

        internal override void ApplySizesTo(Shape shape)
        {
            ((Ellipse)shape).Radii = _radii;
        }

        internal override void Sanitize()
        {
            _segmentCount = ShapeCounts.ClampSegmentCount(_segmentCount);
        }
    }
}
