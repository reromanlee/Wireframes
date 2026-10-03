using System;
using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// Draws a frustum on its GameObject: two regular polygons around the ends of its axis, joined at every corner. Prisms
    /// and regular pyramids are frustums too.
    /// </summary>
    [AddComponentMenu("Wireframes/Frustum")]
    [ExecuteAlways]
    [HelpURL(HelpUrl)]
    public sealed class WireframeFrustum : WireframeAxialShape
    {
        [Tooltip("Distance from end A to the corners of its polygon, in the GameObject's local units.")]
        [SerializeField] private float _radiusA = ShapeDefaults.Radius * 0.5f;

        [Tooltip("Distance from end B to the corners of its polygon, in the GameObject's local units.")]
        [SerializeField] private float _radiusB = ShapeDefaults.Radius;

        [Tooltip("Number of sides of each polygon, from 3 to 1024.")]
        [SerializeField, Delayed] private int _sideCount = FrustumFactory.DefaultSideCount;

        /// <summary>
        /// Distance from end A to the corners of its polygon, in the GameObject's local units. 0.25 by default.
        /// </summary>
        public float RadiusA
        {
            get => _radiusA;
            set
            {
                _radiusA = value;
                if (TryGetShape(out Frustum frustum))
                {
                    frustum.RadiusA = value;
                }
            }
        }

        /// <summary>
        /// Distance from end B to the corners of its polygon, in the GameObject's local units. 0.5 by default.
        /// </summary>
        public float RadiusB
        {
            get => _radiusB;
            set
            {
                _radiusB = value;
                if (TryGetShape(out Frustum frustum))
                {
                    frustum.RadiusB = value;
                }
            }
        }

        /// <summary>
        /// Number of sides of each polygon, from 3 to 1024. 4 by default. Changing it creates the shape again.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The count is outside 3 to 1024.</exception>
        public int SideCount
        {
            get => _sideCount;
            set
            {
                _sideCount = ShapeCounts.CheckSideCount(value);
                Refresh();
            }
        }

        internal override int BuildSignature
        {
            get => _sideCount;
        }

        internal override Shape CreateShape(Transform bone)
        {
            return new Frustum(bone, EndA, AxisRotation, AxisLength, _radiusA, _radiusB, _sideCount);
        }

        internal override void ApplySizesTo(Shape shape)
        {
            Frustum frustum = (Frustum)shape;
            frustum.RadiusA = _radiusA;
            frustum.RadiusB = _radiusB;
        }

        internal override void Sanitize()
        {
            _sideCount = ShapeCounts.ClampSideCount(_sideCount);
        }
    }
}
