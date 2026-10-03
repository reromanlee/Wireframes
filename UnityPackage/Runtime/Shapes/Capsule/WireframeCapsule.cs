using System;
using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// Draws a capsule on its GameObject: two spheres, centered on the ends of its axis, wrapped together like the capsule
    /// of <c>Physics.CapsuleCast</c>. Each end can have its own radius.
    /// </summary>
    [AddComponentMenu("Wireframes/Capsule")]
    [ExecuteAlways]
    [HelpURL(HelpUrl)]
    public sealed class WireframeCapsule : WireframeAxialShape
    {
        [Tooltip("Radius of the sphere around end A, in the GameObject's local units.")]
        [SerializeField] private float _radiusA = ShapeDefaults.Radius;

        [Tooltip("Radius of the sphere around end B, in the GameObject's local units.")]
        [SerializeField] private float _radiusB = ShapeDefaults.Radius;

        [Tooltip("Number of straight pieces each ring is drawn with, a multiple of 4 from 4 to 1024.")]
        [SerializeField, Delayed] private int _segmentCount = Ring.DefaultSegmentCount;

        /// <summary>Radius of the sphere around end A, in the GameObject's local units. 0.5 by default.</summary>
        public float RadiusA
        {
            get => _radiusA;
            set
            {
                _radiusA = value;
                if (TryGetShape(out Capsule capsule))
                {
                    capsule.RadiusA = value;
                }
            }
        }

        /// <summary>Radius of the sphere around end B, in the GameObject's local units. 0.5 by default.</summary>
        public float RadiusB
        {
            get => _radiusB;
            set
            {
                _radiusB = value;
                if (TryGetShape(out Capsule capsule))
                {
                    capsule.RadiusB = value;
                }
            }
        }

        /// <summary>
        /// Number of straight pieces each ring is drawn with, a multiple of 4 from 4 to 1024; each arc over a cap is half
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
            return new Capsule(bone, EndA, AxisRotation, AxisLength, _radiusA, _radiusB, _segmentCount);
        }

        internal override void ApplySizesTo(Shape shape)
        {
            Capsule capsule = (Capsule)shape;
            capsule.RadiusA = _radiusA;
            capsule.RadiusB = _radiusB;
        }

        internal override void Sanitize()
        {
            _segmentCount = ShapeCounts.ClampQuarterSegmentCount(_segmentCount);
        }
    }
}
