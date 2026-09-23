using System;
using UnityEngine;

namespace reromanlee.Wireframes
{
    internal sealed class RoundedRectangle : RectangleShape, IRoundedRectangle
    {
        private const int CornerCount = 4;
        private static readonly PatternCache Patterns = new(BuildEdges);

        private float _cornerRadius;

        internal RoundedRectangle(
            MeshProxy proxy,
            Transform bone,
            Vector3 localCenter,
            Quaternion localRotation,
            Vector2 size,
            float cornerRadius,
            int segments)
            : base(
                proxy,
                Ring.CheckQuarterSegments(segments) + CornerCount,
                Patterns.Get(segments),
                bone,
                localCenter,
                localRotation,
                size)
        {
            _cornerRadius = cornerRadius;
        }

        public float CornerRadius
        {
            get
            {
                ThrowIfDisposed();
                return _cornerRadius;
            }
            set
            {
                ThrowIfDisposed();
                _cornerRadius = value;
                MarkDirty(DirtyFlags.Positions);
            }
        }

        public int Segments
        {
            get
            {
                ThrowIfDisposed();
                return VertexCount - CornerCount;
            }
        }

        protected override void WriteShape(Span<Vector3> positions)
        {
            int segments = positions.Length - CornerCount;
            int quarter = segments / 4;
            Vector2 size = Size;
            // Built for the absolute size, then mirrored by its signs.
            float halfX = Mathf.Abs(size.x) * 0.5f;
            float halfZ = Mathf.Abs(size.y) * 0.5f;
            float mirrorX = size.x < 0f ? -1f : 1f;
            float mirrorZ = size.y < 0f ? -1f : 1f;
            float radius = Mathf.Clamp(_cornerRadius, 0f, Mathf.Min(halfX, halfZ));
            Vector2[] circle = Ring.UnitCircle(segments);

            // Going around from +Z toward +X, the corners are (+X, +Z), (+X, -Z), (-X, -Z) and (-X, +Z); each is a
            // quarter of a circle around a point inset by the radius.
            int vertex = 0;
            for (int corner = 0; corner < CornerCount; corner++)
            {
                float centerX = corner < 2 ? halfX - radius : radius - halfX;
                float centerZ = corner == 0 || corner == 3 ? halfZ - radius : radius - halfZ;
                for (int step = 0; step <= quarter; step++)
                {
                    Vector2 direction = circle[(corner * quarter + step) % segments];
                    positions[vertex++] = new Vector3(
                        (centerX + direction.y * radius) * mirrorX,
                        0f,
                        (centerZ + direction.x * radius) * mirrorZ);
                }
            }
        }

        protected override void ScaleSizes(float factor)
        {
            base.ScaleSizes(factor);
            _cornerRadius *= factor;
        }

        private static int[] BuildEdges(int segments)
        {
            int quarter = segments / 4;
            int cornerVertices = quarter + 1;
            int[] pattern = new int[(segments + CornerCount) * 2];
            int cursor = 0;
            for (int corner = 0; corner < CornerCount; corner++)
            {
                int first = corner * cornerVertices;
                for (int step = 0; step < quarter; step++)
                {
                    pattern[cursor++] = first + step;
                    pattern[cursor++] = first + step + 1;
                }
                // The straight side to the next corner.
                pattern[cursor++] = first + quarter;
                pattern[cursor++] = (corner + 1) % CornerCount * cornerVertices;
            }
            return pattern;
        }
    }
}
