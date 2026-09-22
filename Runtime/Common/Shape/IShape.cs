using UnityEngine;

namespace reromanlee.Wireframes.Common
{
    public interface IShape
    {
        Vector3[] Positions { get; }
        Color[] Colors { get; }
        Transform Parent { get; set; }

        void SetPosition(int index, Vector3 position);
        void SetColor(int index, Color color);
    }

}