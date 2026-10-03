using System;
using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// Draws a stadium on its GameObject, the flat outline of a capsule: two circles centered on the ends of its axis,
    /// wrapped together in the XZ plane of the shape. With no roll it lies as flat on the GameObject as the axis allows.
    /// </summary>
    [AddComponentMenu("Wireframes/Stadium")]
    [ExecuteAlways]
    [HelpURL(HelpUrl)]
    public sealed class WireframeStadium : WireframeAxialShape
    {
        [Tooltip("Radius of the circle around end A, in the GameObject's local units.")]
        [SerializeField] private float _radiusA = ShapeDefaults.Radius;

        [Tooltip("Radius of the circle around end B, in the GameObject's local units.")]
        [SerializeField] private float _radiusB = ShapeDefaults.Radius;

        [Tooltip("Number of straight pieces a full circle would be drawn with, a multiple of 4 from 4 to 1024.")]
        [SerializeField, Delayed] private int _segmentCount = Ring.DefaultSegmentCount;

        /// <summary>Radius of the circle around end A, in the GameObject's local units. 0.5 by default.</summary>
        public float RadiusA
        {
            get => _radiusA;
            set
            {
                _radiusA = value;
                if (TryGetShape(out Stadium stadium))
                {
                    stadium.RadiusA = value;
                }
            }
        }

        /// <summary>Radius of the circle around end B, in the GameObject's local units. 0.5 by default.</summary>
        public float RadiusB
        {
            get => _radiusB;
            set
            {
                _radiusB = value;
                if (TryGetShape(out Stadium stadium))
                {
                    stadium.RadiusB = value;
                }
            }
        }

        /// <summary>
        /// Number of straight pieces a full circle would be drawn with, a multiple of 4 from 4 to 1024; each end is half
        /// of them. 32 by default. Changing it creates the shape again.
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
            return new Stadium(bone, EndA, AxisRotation, AxisLength, _radiusA, _radiusB, _segmentCount);
        }

        internal override void ApplySizesTo(Shape shape)
        {
            Stadium stadium = (Stadium)shape;
            stadium.RadiusA = _radiusA;
            stadium.RadiusB = _radiusB;
        }

        internal override void Sanitize()
        {
            _segmentCount = ShapeCounts.ClampQuarterSegmentCount(_segmentCount);
        }
    }
}
