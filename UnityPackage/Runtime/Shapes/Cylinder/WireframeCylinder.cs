using System;
using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// Draws a cylinder on its GameObject: a ring around each end of its axis, joined by four lines.
    /// </summary>
    [AddComponentMenu("Wireframes/Cylinder")]
    [ExecuteAlways]
    [HelpURL(HelpUrl)]
    public sealed class WireframeCylinder : WireframeAxialShape
    {
        [Tooltip("Radius in the GameObject's local units.")]
        [SerializeField] private float _radius = ShapeDefaults.Radius;

        [Tooltip("Number of straight pieces each ring is drawn with, a multiple of 4 from 4 to 1024.")]
        [SerializeField, Delayed] private int _segmentCount = Ring.DefaultSegmentCount;

        /// <summary>Radius in the GameObject's local units. 0.5 by default.</summary>
        public float Radius
        {
            get => _radius;
            set
            {
                _radius = value;
                if (TryGetShape(out Cylinder cylinder))
                {
                    cylinder.Radius = value;
                }
            }
        }

        /// <summary>
        /// Number of straight pieces each ring is drawn with, a multiple of 4 from 4 to 1024. 32 by default. Changing it
        /// creates the shape again.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The count isn't a multiple of 4 from 4 to 1024.</exception>
        public int SegmentCount
        {
            get => _segmentCount;
            set
            {
                _segmentCount = ShapeCounts.CheckQuarterSegmentCount(value);
                Refresh();
            }
        }

        internal override int BuildSignature
        {
            get => _segmentCount;
        }

        internal override Shape CreateShape(Transform bone)
        {
            return new Cylinder(bone, EndA, AxisRotation, AxisLength, _radius, _segmentCount);
        }

        internal override void ApplySizesTo(Shape shape)
        {
            ((Cylinder)shape).Radius = _radius;
        }

        internal override void Sanitize()
        {
            _segmentCount = ShapeCounts.ClampQuarterSegmentCount(_segmentCount);
        }
    }
}
