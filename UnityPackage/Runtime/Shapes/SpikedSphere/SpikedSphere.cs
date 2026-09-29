using System;
using UnityEngine;

namespace reromanlee.Wireframes
{
    internal sealed class SpikedSphere : RigidShape, ISpikedSphere
    {
        private readonly PlatonicSolid _solid;
        private float _baseRadius;
        private float _spikeLength;

        internal SpikedSphere(
            Transform bone,
            Vector3 localCenter,
            Quaternion localRotation,
            float baseRadius,
            float spikeLength,
            int spikeCount)
            : this(bone, localCenter, localRotation, baseRadius, spikeLength, SolidFor(spikeCount))
        {
        }

        private SpikedSphere(
            Transform bone,
            Vector3 localCenter,
            Quaternion localRotation,
            float baseRadius,
            float spikeLength,
            PlatonicSolid solid)
            : base(
                solid.Corners.Length + solid.Faces.Length,
                solid.EdgePattern,
                bone,
                localCenter,
                localRotation)
        {
            _solid = solid;
            _baseRadius = baseRadius;
            _spikeLength = spikeLength;
        }

        public float BaseRadius
        {
            get
            {
                EnsureUsable();
                return _baseRadius;
            }
            set
            {
                EnsureUsable();
                _baseRadius = value;
                MarkDirty(DirtyFlags.Positions);
            }
        }

        public float SpikeLength
        {
            get
            {
                EnsureUsable();
                return _spikeLength;
            }
            set
            {
                EnsureUsable();
                _spikeLength = value;
                MarkDirty(DirtyFlags.Positions);
            }
        }

        public int SpikeCount
        {
            get
            {
                EnsureUsable();
                return _solid.Faces.Length;
            }
        }

        protected override void WriteShape(Span<Vector3> positions)
        {
            Vector3[] corners = _solid.Corners;
            Vector3[] faces = _solid.Faces;
            for (int i = 0; i < corners.Length; i++)
            {
                positions[i] = corners[i] * _baseRadius;
            }
            float tipDistance = _baseRadius + _spikeLength;
            for (int i = 0; i < faces.Length; i++)
            {
                positions[corners.Length + i] = faces[i] * tipDistance;
            }
        }

        protected override void ScaleSizes(float factor)
        {
            _baseRadius *= factor;
            _spikeLength *= factor;
        }

        private static PlatonicSolid SolidFor(int spikeCount)
        {
            if (!PlatonicSolid.HasFaceCount(spikeCount))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(spikeCount), spikeCount, "A spiked sphere needs 4, 6, 8, 12 or 20 spikes.");
            }
            return PlatonicSolid.WithFaces(spikeCount);
        }
    }
}
