using System;
using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// Draws a flat rectangle with round corners on its GameObject, centered on <see cref="WireframeCenteredShape.Center"/>
    /// in the XZ plane of its rotation.
    /// </summary>
    [AddComponentMenu("Wireframes/Rounded Rectangle")]
    [ExecuteAlways]
    [HelpURL(HelpUrl)]
    public sealed class WireframeRoundedRectangle : WireframeCenteredShape
    {
        [Tooltip("Width along the rectangle's X axis and depth along its Z axis, in the GameObject's local units.")]
        [SerializeField] private Vector2 _size = Vector2.one * ShapeDefaults.Size;

        [Tooltip("Radius of the corners, drawn between 0 and half the shorter side.")]
        [SerializeField] private float _cornerRadius = RoundedRectangleFactory.DefaultCornerRadius;

        [Tooltip("Number of straight pieces a full circle of the corners is drawn with, a multiple of 4 from 4 to 1024.")]
        [SerializeField, Delayed] private int _segmentCount = Ring.DefaultSegmentCount;

        /// <summary>
        /// Width along the rectangle's X axis and depth along its Z axis, in the GameObject's local units. 1 by 1 by
        /// default.
        /// </summary>
        public Vector2 Size
        {
            get => _size;
            set
            {
                _size = value;
                if (TryGetShape(out RoundedRectangle rectangle))
                {
                    rectangle.Size = value;
                }
            }
        }

        /// <summary>
        /// Radius of the corners in the GameObject's local units, drawn clamped between 0, for sharp corners, and half the
        /// shorter side. 0.25 by default.
        /// </summary>
        public float CornerRadius
        {
            get => _cornerRadius;
            set
            {
                _cornerRadius = value;
                if (TryGetShape(out RoundedRectangle rectangle))
                {
                    rectangle.CornerRadius = value;
                }
            }
        }

        /// <summary>
        /// Number of straight pieces a full circle of the corners is drawn with, a multiple of 4 from 4 to 1024; each
        /// corner is a quarter of them. 32 by default. Changing it creates the shape again.
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
            return new RoundedRectangle(bone, Center, Rotation, _size, _cornerRadius, _segmentCount);
        }

        internal override void ApplySizesTo(Shape shape)
        {
            RoundedRectangle rectangle = (RoundedRectangle)shape;
            rectangle.Size = _size;
            rectangle.CornerRadius = _cornerRadius;
        }

        internal override void Sanitize()
        {
            _segmentCount = ShapeCounts.ClampQuarterSegmentCount(_segmentCount);
        }
    }
}
