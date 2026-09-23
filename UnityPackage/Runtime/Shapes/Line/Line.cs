using UnityEngine;

namespace reromanlee.Wireframes
{
    internal sealed class Line : PointShape, ILine
    {
        private const int LineVertexCount = 2;
        private static readonly int[] EdgePattern = { 0, 1 };

        private ShapePoint _pointA;
        private ShapePoint _pointB;

        internal Line(MeshProxy proxy, Vector3 positionA, Vector3 positionB) : base(proxy, LineVertexCount, EdgePattern)
        {
            _pointA = new ShapePoint(positionA);
            _pointB = new ShapePoint(positionB);
        }

        /// <summary>Creates a line whose endpoints sit at the origins of their bones.</summary>
        internal Line(MeshProxy proxy, Transform boneA, Transform boneB) : this(proxy, Vector3.zero, Vector3.zero)
        {
            AttachPoint(0, boneA);
            AttachPoint(1, boneB);
        }

        public Vector3 LocalPositionA
        {
            get => GetLocalPosition(0);
            set => SetLocalPosition(0, value);
        }

        public Vector3 LocalPositionB
        {
            get => GetLocalPosition(1);
            set => SetLocalPosition(1, value);
        }

        public Vector3 WorldPositionA
        {
            get => GetWorldPosition(0);
            set => SetWorldPosition(0, value);
        }

        public Vector3 WorldPositionB
        {
            get => GetWorldPosition(1);
            set => SetWorldPosition(1, value);
        }

        public Color ColorA
        {
            get => GetColor(0);
            set => SetColor(0, value);
        }

        public Color ColorB
        {
            get => GetColor(1);
            set => SetColor(1, value);
        }

        public Transform BoneA
        {
            get => GetBone(0);
            set => SetBone(0, value);
        }

        public Transform BoneB
        {
            get => GetBone(1);
            set => SetBone(1, value);
        }

        protected override ref ShapePoint Point(int index)
        {
            if (index == 0)
            {
                return ref _pointA;
            }
            return ref _pointB;
        }
    }
}
