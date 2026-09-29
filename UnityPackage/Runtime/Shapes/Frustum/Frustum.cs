using System;
using UnityEngine;

namespace reromanlee.Wireframes
{
    internal sealed class Frustum : AxialShape, IFrustum
    {
        private const int PolygonCount = 2;
        private static readonly PatternCache Patterns = new(BuildEdges);

        private float _radiusA;
        private float _radiusB;

        internal Frustum(
            Transform bone,
            Vector3 localPosition,
            Quaternion localRotation,
            float length,
            float radiusA,
            float radiusB,
            int sideCount)
            : base(CountVertices(sideCount), Patterns.Get(sideCount), bone, localPosition, localRotation, length)
        {
            _radiusA = radiusA;
            _radiusB = radiusB;
        }

        public float RadiusA
        {
            get
            {
                EnsureUsable();
                return _radiusA;
            }
            set
            {
                EnsureUsable();
                _radiusA = value;
                MarkDirty(DirtyFlags.Positions);
            }
        }

        public float RadiusB
        {
            get
            {
                EnsureUsable();
                return _radiusB;
            }
            set
            {
                EnsureUsable();
                _radiusB = value;
                MarkDirty(DirtyFlags.Positions);
            }
        }

        public int SideCount
        {
            get
            {
                EnsureUsable();
                return VertexCount / PolygonCount;
            }
        }

        protected override void WriteShape(Span<Vector3> positions, float length)
        {
            int sideCount = positions.Length / PolygonCount;
            for (int corner = 0; corner < sideCount; corner++)
            {
                // Half a side past -Y, so the bottom side is level.
                float angle = Mathf.PI * (2f * corner + 1f) / sideCount - Mathf.PI * 0.5f;
                float cos = Mathf.Cos(angle);
                float sin = Mathf.Sin(angle);
                positions[corner] = new Vector3(cos * _radiusA, sin * _radiusA, 0f);
                positions[sideCount + corner] = new Vector3(cos * _radiusB, sin * _radiusB, length);
            }
        }

        protected override void ScaleCrossSection(float factor)
        {
            _radiusA *= factor;
            _radiusB *= factor;
        }

        private static int CountVertices(int sideCount)
        {
            if (sideCount < 3 || sideCount > Ring.MaxSegmentCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(sideCount), sideCount, $"A frustum needs 3 to {Ring.MaxSegmentCount} sides.");
            }
            return sideCount * PolygonCount;
        }

        private static int[] BuildEdges(int sideCount)
        {
            int[] pattern = new int[sideCount * 3 * 2];
            int cursor = 0;
            Ring.AddEdges(pattern, ref cursor, 0, sideCount);
            Ring.AddEdges(pattern, ref cursor, sideCount, sideCount);
            for (int corner = 0; corner < sideCount; corner++)
            {
                pattern[cursor++] = corner;
                pattern[cursor++] = sideCount + corner;
            }
            return pattern;
        }
    }
}
