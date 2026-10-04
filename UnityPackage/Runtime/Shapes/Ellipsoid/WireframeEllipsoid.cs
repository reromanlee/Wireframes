using System;
using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// Draws an ellipsoid on its GameObject as three ellipses around <see cref="WireframeCenteredShape.Center"/>, one in
    /// each plane of its axes.
    /// </summary>
    [AddComponentMenu("Wireframes/Ellipsoid")]
    [ExecuteAlways]
    [HelpURL(HelpUrl)]
    public sealed class WireframeEllipsoid : WireframeCenteredShape
    {
        [Tooltip("Radius along the ellipsoid's X, Y and Z axes, in the GameObject's local units.")]
        [SerializeField] private Vector3 _radii =
            new(ShapeDefaults.Radius * 0.5f, ShapeDefaults.Radius * 0.5f, ShapeDefaults.Radius);

        [Tooltip("Number of straight pieces each ellipse is drawn with, from 3 to 1024.")]
        [SerializeField, Delayed] private int _segmentCount = Ring.DefaultSegmentCount;

        /// <summary>
        /// Semi-axes along the ellipsoid's X, Y and Z axes, in the GameObject's local units. 0.25, 0.25 and 0.5 by
        /// default.
        /// </summary>
        public Vector3 Radii
        {
            get => _radii;
            set
            {
                _radii = value;
                if (TryGetShape(out Ellipsoid ellipsoid))
                {
                    ellipsoid.Radii = value;
                }
            }
        }

        /// <summary>
        /// Number of straight pieces each ellipse is drawn with, from 3 to 1024. 32 by default. Changing it creates the
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
            return new Ellipsoid(bone, Center, Rotation, _radii, _segmentCount);
        }

        internal override void ApplySizesTo(Shape shape)
        {
            ((Ellipsoid)shape).Radii = _radii;
        }

        internal override void Sanitize()
        {
            _segmentCount = ShapeCounts.ClampSegmentCount(_segmentCount);
        }
    }
}
