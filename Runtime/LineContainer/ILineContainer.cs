using reromanlee.Wireframes.Common;
using UnityEngine;

namespace reromanlee.Wireframes
{

    public interface ILineContainer
    {

        ILine CreateLine();
        ILine CreateLine(Vector3 positionA, Vector3 positionB);

        IBox CreateBox();
        IBox CreateBox(Vector3 cornerA, Vector3 cornerB);

    }

}