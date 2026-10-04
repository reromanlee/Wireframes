using System;
using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// Draws a cone on its GameObject, with its tip at <see cref="WireframeAxialShape.EndA"/> and the center of its base at
    /// <see cref="WireframeAxialShape.EndB"/>: a base ring joined to the tip by four lines.
    /// </summary>
    [AddComponentMenu("Wireframes/Cone")]
    [ExecuteAlways]
    [HelpURL(HelpUrl)]
    public sealed class WireframeCone : WireframeAxialShape
    {
        [Tooltip("Radius of the base in the GameObject's local units.")]
        [SerializeField] private float _radius = ShapeDefaults.Radius;

        [Tooltip("Number of straight pieces the base ring is drawn with, a multiple of 4 from 4 to 1024.")]
        [SerializeField, Delayed] private int _segmentCount = Ring.DefaultSegmentCount;

        /// <summary>Radius of the base in the GameObject's local units. 0.5 by default.</summary>
        public float Radius
        {
            get => _radius;
            set
            {
                _radius = value;
                if (TryGetShape(out Cone cone))
                {
                    cone.Radius = value;
                }
            }
        }

        /// <summary>
        /// Number of straight pieces the base ring is drawn with, a multiple of 4 from 4 to 1024. 32 by default. Changing
        /// it creates the shape again.
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
            return new Cone(bone, EndA, AxisRotation, AxisLength, _radius, _segmentCount);
        }

        internal override void ApplySizesTo(Shape shape)
        {
            ((Cone)shape).Radius = _radius;
        }

        internal override void Sanitize()
        {
            _segmentCount = ShapeCounts.ClampQuarterSegmentCount(_segmentCount);
        }
    }
}
