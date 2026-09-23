using System;
using UnityEngine;

namespace reromanlee.Wireframes
{
    internal sealed class Stadium : AxialShape, IStadium
    {
        private const int ArcCount = 2;

        private float _radiusA;
        private float _radiusB;

        internal Stadium(
            MeshProxy proxy,
            Transform bone,
            Vector3 localPosition,
            Quaternion localRotation,
            float length,
            float radiusA,
            float radiusB,
            int segments)
            : base(
                proxy,
                Ring.CheckQuarterSegments(segments) + ArcCount,
                // One closed loop: arc A, the -X side, arc B and the +X side back to the start.
                Ring.Patterns.Get(segments + ArcCount),
                bone,
                localPosition,
                localRotation,
                length)
        {
            _radiusA = radiusA;
            _radiusB = radiusB;
        }

        public float RadiusA
        {
            get
            {
                ThrowIfDisposed();
                return _radiusA;
            }
            set
            {
                ThrowIfDisposed();
                _radiusA = value;
                MarkDirty(DirtyFlags.Positions);
            }
        }

        public float RadiusB
        {
            get
            {
                ThrowIfDisposed();
                return _radiusB;
            }
            set
            {
                ThrowIfDisposed();
                _radiusB = value;
                MarkDirty(DirtyFlags.Positions);
            }
        }

        public int Segments
        {
            get
            {
                ThrowIfDisposed();
                return VertexCount - ArcCount;
            }
        }

        // Arc A runs from the +X side around behind end A to the -X side, then arc B from the -X side around past end B
        // back to +X; the straight sides join them.
        protected override void WriteShape(Span<Vector3> positions, float length)
        {
            int half = (positions.Length - ArcCount) / 2;
            // Built along +Z for the sizes without their signs, then mirrored when the length is negative.
            float span = Mathf.Abs(length);
            float radiusA = Mathf.Abs(_radiusA);
            float radiusB = Mathf.Abs(_radiusB);
            Vector3 centerB = new(0f, 0f, span);
            Hull.Tangent(radiusA, radiusB, span, out float sine, out float cosine);
            float edgeA = Mathf.Atan2(cosine, sine);
            float edgeB = Mathf.Atan2(cosine, -sine);

            Span<Vector3> arcA = positions.Slice(0, half + 1);
            Span<Vector3> arcB = positions.Slice(half + 1, half + 1);
            for (int step = 0; step <= half; step++)
            {
                float along = 1f - 2f * step / half;
                arcA[step] = Hull.ArcPoint(Vector3.zero, radiusA, Vector3.right, Vector3.back, edgeA * along);
                arcB[step] = Hull.ArcPoint(centerB, radiusB, Vector3.right, Vector3.forward, -edgeB * along);
            }

            if (cosine == 0f)
            {
                // One circle holds the other: the bigger one's arc is a full circle, and the smaller one folds onto
                // the point where that circle starts and ends.
                if (sine < 0f)
                {
                    arcB.Fill(arcA[0]);
                }
                else
                {
                    arcA.Fill(arcB[0]);
                }
            }
            if (length < 0f)
            {
                for (int i = 0; i < positions.Length; i++)
                {
                    positions[i].z = -positions[i].z;
                }
            }
        }

        protected override void ScaleCrossSection(float factor)
        {
            _radiusA *= factor;
            _radiusB *= factor;
        }
    }
}
