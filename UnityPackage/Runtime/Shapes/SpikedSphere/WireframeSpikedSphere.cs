using System;
using UnityEngine;

namespace reromanlee.Wireframes
{
    /// <summary>
    /// Draws a spiked sphere on its GameObject, a 3D star around <see cref="WireframeCenteredShape.Center"/>: a Platonic
    /// solid with a spike on each face.
    /// </summary>
    [AddComponentMenu("Wireframes/Spiked Sphere")]
    [ExecuteAlways]
    [HelpURL(HelpUrl)]
    public sealed class WireframeSpikedSphere : WireframeCenteredShape
    {
        [Tooltip("Distance from the center to the corners of the solid, in the GameObject's local units.")]
        [SerializeField] private float _baseRadius = ShapeDefaults.Radius * 0.5f;

        [Tooltip("How far the spike tips reach beyond the base radius, in the GameObject's local units.")]
        [SerializeField] private float _spikeLength = ShapeDefaults.Radius * 0.5f;

        [Tooltip("Number of spikes, one on each face of a Platonic solid: 4, 6, 8, 12 or 20.")]
        [SerializeField] private int _spikeCount = SpikedSphereFactory.DefaultSpikeCount;

        /// <summary>
        /// Distance from the center to the corners of the solid, in the GameObject's local units. 0.25 by default.
        /// </summary>
        public float BaseRadius
        {
            get => _baseRadius;
            set
            {
                _baseRadius = value;
                if (TryGetShape(out SpikedSphere sphere))
                {
                    sphere.BaseRadius = value;
                }
            }
        }

        /// <summary>
        /// How far the spike tips reach beyond <see cref="BaseRadius"/>, in the GameObject's local units. 0.25 by
        /// default.
        /// </summary>
        public float SpikeLength
        {
            get => _spikeLength;
            set
            {
                _spikeLength = value;
                if (TryGetShape(out SpikedSphere sphere))
                {
                    sphere.SpikeLength = value;
                }
            }
        }

        /// <summary>
        /// Number of spikes: 4, 6, 8, 12 or 20, one on each face of a tetrahedron, cube, octahedron, dodecahedron or
        /// icosahedron. 12 by default. Changing it creates the shape again.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The count isn't 4, 6, 8, 12 or 20.</exception>
        public int SpikeCount
        {
            get => _spikeCount;
            set
            {
                _spikeCount = ShapeCounts.CheckSpikeCount(value);
                Refresh();
            }
        }

        internal override int BuildSignature
        {
            get => _spikeCount;
        }

        internal override Shape CreateShape(Transform bone)
        {
            return new SpikedSphere(bone, Center, Rotation, _baseRadius, _spikeLength, _spikeCount);
        }

        internal override void ApplySizesTo(Shape shape)
        {
            SpikedSphere sphere = (SpikedSphere)shape;
            sphere.BaseRadius = _baseRadius;
            sphere.SpikeLength = _spikeLength;
        }

        internal override void Sanitize()
        {
            _spikeCount = ShapeCounts.ClampSpikeCount(_spikeCount);
        }
    }
}
