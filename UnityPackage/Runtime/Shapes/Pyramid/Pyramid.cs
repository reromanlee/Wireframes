using System;
using UnityEngine;

namespace reromanlee.Wireframes
{
    internal sealed class Pyramid : AxialShape, IPyramid
    {
        private const int PyramidVertexCount = 5;

        // Vertex 0 is the tip, 1 to 4 go around the base.
        private static readonly int[] EdgePattern =
        {
            1, 2, 2, 3, 3, 4, 4, 1,
            0, 1, 0, 2, 0, 3, 0, 4
        };

        private Vector2 _baseSize;

        internal Pyramid(
            MeshProxy proxy,
            Transform bone,
            Vector3 localPosition,
            Quaternion localRotation,
            float length,
            Vector2 baseSize) : base(proxy, PyramidVertexCount, EdgePattern, bone, localPosition, localRotation, length)
        {
            _baseSize = baseSize;
        }

        public Vector2 BaseSize
        {
            get
            {
                ThrowIfDisposed();
                return _baseSize;
            }
            set
            {
                ThrowIfDisposed();
                _baseSize = value;
                MarkDirty(DirtyFlags.Positions);
            }
        }

        protected override void WriteShape(Span<Vector3> positions, float length)
        {
            Vector2 half = _baseSize * 0.5f;
            positions[0] = Vector3.zero;
            positions[1] = new Vector3(-half.x, -half.y, length);
            positions[2] = new Vector3(half.x, -half.y, length);
            positions[3] = new Vector3(half.x, half.y, length);
            positions[4] = new Vector3(-half.x, half.y, length);
        }

        protected override void ScaleCrossSection(float factor)
        {
            _baseSize *= factor;
        }
    }
}
