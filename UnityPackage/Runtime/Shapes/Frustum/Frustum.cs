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
            MeshProxy proxy,
            Transform bone,
            Vector3 localPosition,
            Quaternion localRotation,
            float length,
            float radiusA,
            float radiusB,
            int sides)
            : base(proxy, CountVertices(sides), Patterns.Get(sides), bone, localPosition, localRotation, length)
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

        public int Sides
        {
            get
            {
                ThrowIfDisposed();
                return VertexCount / PolygonCount;
            }
        }

        protected override void WriteShape(Span<Vector3> positions, float length)
        {
            int sides = positions.Length / PolygonCount;
            for (int corner = 0; corner < sides; corner++)
            {
                // Half a side past -Y, so the bottom side is level.
                float angle = Mathf.PI * (2f * corner + 1f) / sides - Mathf.PI * 0.5f;
                float cos = Mathf.Cos(angle);
                float sin = Mathf.Sin(angle);
                positions[corner] = new Vector3(cos * _radiusA, sin * _radiusA, 0f);
                positions[sides + corner] = new Vector3(cos * _radiusB, sin * _radiusB, length);
            }
        }

        protected override void ScaleCrossSection(float factor)
        {
            _radiusA *= factor;
            _radiusB *= factor;
        }

        private static int CountVertices(int sides)
        {
            if (sides < 3)
            {
                throw new ArgumentOutOfRangeException(nameof(sides), sides, "A frustum needs at least 3 sides.");
            }
            return sides * PolygonCount;
        }

        private static int[] BuildEdges(int sides)
        {
            int[] pattern = new int[sides * 3 * 2];
            int cursor = 0;
            Ring.AddEdges(pattern, ref cursor, 0, sides);
            Ring.AddEdges(pattern, ref cursor, sides, sides);
            for (int corner = 0; corner < sides; corner++)
            {
                pattern[cursor++] = corner;
                pattern[cursor++] = sides + corner;
            }
            return pattern;
        }
    }
}
