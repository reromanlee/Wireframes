using UnityEngine;

namespace reromanlee.Wireframes.Common
{

    public interface ILine
    {

        Vector3 PositionA { get; set; }
        Vector3 PositionB { get; set; }

        Color ColorA { get; set; }
        Color ColorB { get; set; }

        Transform BoneA { get; set; }
        Transform BoneB { get; set; }

    }

}