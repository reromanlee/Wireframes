using System;
using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// Draws a flat circle on its GameObject, centered on <see cref="WireframeCenteredShape.Center"/> in the XZ plane of
    /// its rotation.
    /// </summary>
    [AddComponentMenu("Wireframes/Circle")]
    [ExecuteAlways]
    [HelpURL(HelpUrl)]
    public sealed class WireframeCircle : WireframeCenteredShape
    {
        [Tooltip("Radius in the GameObject's local units.")]
        [SerializeField] private float _radius = ShapeDefaults.Radius;

        [Tooltip("Number of straight pieces the circle is drawn with, from 3 to 1024.")]
        [SerializeField, Delayed] private int _segmentCount = Ring.DefaultSegmentCount;

        /// <summary>Radius in the GameObject's local units. 0.5 by default.</summary>
        public float Radius
        {
            get => _radius;
            set
            {
                _radius = value;
                if (TryGetShape(out Circle circle))
                {
                    circle.Radius = value;
                }
            }
        }

        /// <summary>
        /// Number of straight pieces the circle is drawn with, from 3 to 1024. 32 by default. Changing it creates the
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
            return new Circle(bone, Center, Rotation, _radius, _segmentCount);
        }

        internal override void ApplySizesTo(Shape shape)
        {
            ((Circle)shape).Radius = _radius;
        }

        internal override void Sanitize()
        {
            _segmentCount = ShapeCounts.ClampSegmentCount(_segmentCount);
        }
    }
}
