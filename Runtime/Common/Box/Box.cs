using UnityEngine;

namespace reromanlee.Wireframes.Common
{
    public class Box : IBox
    {
        private static readonly Vector3 DefaultPosition = Vector3.zero;
        private static readonly Color DefaultColor = Color.white;

        public Box(Transform rootBone)
        {

        }

        public Box(Transform rootBone, Vector3 cornerA, Vector3 cornerB)
        {
            Positions = CreateBoxVertices(cornerA, cornerB);
            Colors = CreateBoxColors();
        }

        private Vector3[] CreateBoxVertices(Vector3 corner0, Vector3 corner7)
        {
            Vector3 corner1 = new(corner7.x, corner0.y, corner0.z);
            Vector3 corner2 = new(corner0.x, corner0.y, corner7.z);
            Vector3 corner3 = new(corner7.x, corner0.y, corner7.z);
            Vector3 corner4 = new(corner0.x, corner7.y, corner0.z);
            Vector3 corner5 = new(corner7.x, corner7.y, corner0.z);
            Vector3 corner6 = new(corner0.x, corner7.y, corner7.z);

            return new Vector3[24] {
                corner0, corner1,
                corner1, corner3,
                corner0, corner2,
                corner2, corner3,

                corner0, corner4,
                corner1, corner5,
                corner2, corner6,
                corner3, corner7,

                corner4, corner5,
                corner5, corner7,
                corner4, corner6,
                corner6, corner7
            };
        }

        private Color[] CreateBoxColors()
        {
            Color[] colors = new Color[24];
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = DefaultColor;
            }
            return colors;
        }

        public Vector3 CornerA
        {
            get
            {
                return default;
            }
            set
            {

            }
        }

        public Vector3 CornerB
        {
            get
            {
                return default;
            }
            set
            {

            }
        }

        public Vector3[] Positions { get; }

        public void SetPosition(int index, Vector3 position)
        {

        }

        public Color[] Colors { get; }

        public void SetColor(int index, Color color)
        {

        }

        public Transform Parent
        {
            get
            {
                return default;
            }
            set
            {

            }
        }
    }
}