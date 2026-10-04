using System;
using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// Draws a flat star on its GameObject, centered on <see cref="WireframeCenteredShape.Center"/> in the XZ plane of its
    /// rotation, with a tip on its +Z axis.
    /// </summary>
    [AddComponentMenu("Wireframes/Star")]
    [ExecuteAlways]
    [HelpURL(HelpUrl)]
    public sealed class WireframeStar : WireframeCenteredShape
    {
        [Tooltip("Distance from the center to the corners between the points, in the GameObject's local units.")]
        [SerializeField] private float _innerRadius = StarFactory.DefaultInnerRadius;

        [Tooltip("Distance from the center to the tips of the points, in the GameObject's local units.")]
        [SerializeField] private float _outerRadius = ShapeDefaults.Radius;

        [Tooltip("Number of points, from 3 to 512.")]
        [SerializeField, Delayed] private int _pointCount = StarFactory.DefaultPointCount;

        /// <summary>
        /// Distance from the center to the corners between the points, in the GameObject's local units. 0.2 by default.
        /// </summary>
        public float InnerRadius
        {
            get => _innerRadius;
            set
            {
                _innerRadius = value;
                if (TryGetShape(out Star star))
                {
                    star.InnerRadius = value;
                }
            }
        }

        /// <summary>
        /// Distance from the center to the tips of the points, in the GameObject's local units. 0.5 by default.
        /// </summary>
        public float OuterRadius
        {
            get => _outerRadius;
            set
            {
                _outerRadius = value;
                if (TryGetShape(out Star star))
                {
                    star.OuterRadius = value;
                }
            }
        }

        /// <summary>Number of points, from 3 to 512. 5 by default. Changing it creates the shape again.</summary>
        /// <exception cref="ArgumentOutOfRangeException">The count is outside 3 to 512.</exception>
        public int PointCount
        {
            get => _pointCount;
            set
            {
                _pointCount = ShapeCounts.CheckPointCount(value);
                Refresh();
            }
        }

        internal override int BuildSignature
        {
            get => _pointCount;
        }

        internal override Shape CreateShape(Transform bone)
        {
            return new Star(bone, Center, Rotation, _innerRadius, _outerRadius, _pointCount);
        }

        internal override void ApplySizesTo(Shape shape)
        {
            Star star = (Star)shape;
            star.InnerRadius = _innerRadius;
            star.OuterRadius = _outerRadius;
        }

        internal override void Sanitize()
        {
            _pointCount = ShapeCounts.ClampPointCount(_pointCount);
        }
    }
}
