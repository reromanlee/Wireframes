using System;
using UnityEngine;

namespace reromanlee.Wireframes
{
    internal sealed class Rectangle : RectangleShape, IRectangle
    {
        private const int CornerCount = 4;

        // Corner 0 is A and corner 2 is B.
        private static readonly int[] EdgePattern = { 0, 1, 1, 2, 2, 3, 3, 0 };

        internal Rectangle(MeshProxy proxy, Transform bone, Vector3 localCenter, Quaternion localRotation, Vector2 size)
            : base(proxy, CornerCount, EdgePattern, bone, localCenter, localRotation, size)
        {
        }

        protected override void WriteShape(Span<Vector3> positions)
        {
            Vector2 half = Size * 0.5f;
            positions[0] = new Vector3(-half.x, 0f, -half.y);
            positions[1] = new Vector3(half.x, 0f, -half.y);
            positions[2] = new Vector3(half.x, 0f, half.y);
            positions[3] = new Vector3(-half.x, 0f, half.y);
        }
    }
}
