using UnityEngine;

namespace reromanlee.Wireframes.Common
{

    public interface IBox : IShape
    {

        public Vector3 CornerA { get; set; }
        public Vector3 CornerB { get; set; }

    }

}